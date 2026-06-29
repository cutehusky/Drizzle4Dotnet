using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Drizzle4Dotnet.Cli.Models;
using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'apply' command: applies pending SQL migration scripts
/// from the migration journal to a database using the CLI's MigrationManager.
/// Each migration is tracked with status (running/success/failed) and
/// execution logs. Wraps each migration in a transaction when possible.
/// </summary>
public static class ApplyCommand
{
    /// <summary>
    /// Maps provider names to known DbConnection type names for automatic creation.
    /// </summary>
    private static readonly Dictionary<string, string> ConnectionTypeMap = new()
    {
        ["pgsql"] = "Npgsql.NpgsqlConnection, Npgsql",
        ["mysql"] = "MySql.Data.MySqlClient.MySqlConnection, MySql.Data",
        ["mssql"] = "Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient",
        ["sqlite"] = "Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite",
        ["oracle"] = "Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess",
    };

    public static async Task<int> Execute(ApplyOptions options)
    {
        try
        {
            // Resolve provider connection type
            var provider = options.Provider.ToLowerInvariant();
            if (!ConnectionTypeMap.TryGetValue(provider, out var connectionTypeName))
            {
                Console.Error.WriteLine($"❌ Unknown provider '{options.Provider}'.");
                Console.Error.WriteLine($"   Supported providers: {string.Join(", ", ConnectionTypeMap.Keys)}");
                return 1;
            }

            // Resolve migrations directory
            var outputDir = ResolveOutputDir(options.OutputDir, provider);
            var journalPath = Path.Combine(outputDir, "migration-journal.json");

            if (!File.Exists(journalPath))
            {
                Console.Error.WriteLine($"❌ Migration journal not found at: {journalPath}");
                Console.Error.WriteLine("   Generate migrations first using 'drizzle4net generate'.");
                return 1;
            }

            // Load journal
            var journal = MigrationJournal.Load(journalPath);
            if (journal.Migrations.Count == 0)
            {
                Console.WriteLine("ℹ️  No migrations found in journal. Nothing to apply.");
                return 0;
            }

            Console.WriteLine($"📋 Migration Journal Loaded");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {options.Provider}");
            Console.WriteLine($"  Total:        {journal.Migrations.Count} migration(s)");
            Console.WriteLine($"  Directory:    {Path.GetFullPath(outputDir)}");
            Console.WriteLine($"  Tracking:     {options.MigrationSchema}.{options.MigrationTable}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Create database connection
            Console.WriteLine($"🔌 Connecting to database...");

            DbConnection connection;
            try
            {
                connection = CreateConnection(connectionTypeName, options.ConnectionString);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Failed to create database connection: {ex.Message}");
                Console.Error.WriteLine($"   Ensure the ADO.NET provider package is installed.");
                return 1;
            }

            await using (connection)
            {
                await connection.OpenAsync();

                // Create the CLI MigrationManager directly — no generics, no dynamic
                var manager = new MigrationManager(
                    connection,
                    null, // no external transaction — we manage our own per migration
                    options.MigrationSchema,
                    options.MigrationTable
                );

                // Get already successful applied migrations from database
                Console.WriteLine($"🔍 Checking applied migrations...");
                var appliedMigrations = await manager.GetAppliedMigrationsAsync();

                Console.WriteLine($"  Applied:      {appliedMigrations.Count} migration(s) in database");

                // Verify checksums of already-applied migrations against the journal
                var checksumErrors = false;
                foreach (var entry in journal.Migrations)
                {
                    var applied = appliedMigrations.FirstOrDefault(a => a.MigrationId == entry.IncrementalId);
                    if (applied.Id == 0) continue;

                    // Compare the database checksum with the journal checksum
                    if (!string.Equals(applied.Checksum, entry.Checksum, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine($"❌ Checksum mismatch for applied migration #{entry.IncrementalId} ({entry.Name})");
                        Console.Error.WriteLine($"   Database:  {applied.Checksum}");
                        Console.Error.WriteLine($"   Journal:   {entry.Checksum}");
                        Console.Error.WriteLine("   The migration may have been tampered with after being applied.");
                        checksumErrors = true;
                    }
                }
                
                if (checksumErrors)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("❌ Checksum verification failed. Aborting to prevent data inconsistency.");
                    return 1;
                }

                // Determine pending migrations — match by MigrationId (IncrementalId)
                var pending = new List<(MigrationJournalEntry Entry, string SqlContent)>();
                foreach (var entry in journal.Migrations)
                {
                    if (appliedMigrations.Any(a => a.MigrationId == entry.IncrementalId))
                        continue;

                    // Read the SQL file
                    var sqlPath = Path.Combine(outputDir, entry.SqlFileName);
                    if (!File.Exists(sqlPath))
                    {
                        Console.Error.WriteLine($"⚠️  SQL file not found: {entry.SqlFileName} — skipping");
                        continue;
                    }

                    var sqlContent = await File.ReadAllTextAsync(sqlPath);
                    pending.Add((entry, sqlContent));
                }

                if (pending.Count == 0)
                {
                    Console.WriteLine($"✅ All {journal.Migrations.Count} migration(s) already applied. Database is up to date.");
                    return 0;
                }

                Console.WriteLine($"  Pending:      {pending.Count} migration(s) to apply");
                Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // Apply pending migrations in order
                var totalApplied = 0;
                var totalFailed = 0;

                foreach (var (entry, sqlContent) in pending)
                {
                    Console.WriteLine();
                    Console.WriteLine($"⚡ Applying:    {entry.Name} ({entry.SqlFileName})");

                    // Try to begin a transaction for this migration
                    DbTransaction? migrationTx = null;
                    try
                    {
                        migrationTx = await connection.BeginTransactionAsync();
                    }
                    catch
                    {
                        Console.WriteLine("  ⚠  Transaction not supported, running without transaction");
                    }

                    try
                    {
                        // Create a manager WITH the transaction (if any)
                        var txManager = new MigrationManager(
                            connection,
                            migrationTx,
                            options.MigrationSchema,
                            options.MigrationTable
                        );

                        // Step 1: Insert "running" record (use IncrementalId as MigrationId)
                        var recordId = await txManager.StartMigrationAsync(entry.IncrementalId, entry.Name, entry.Checksum);
                        Console.WriteLine($"  📝 Record:     #{recordId} [{entry.IncrementalId}] (status: running)");

                        // Step 2: Execute the raw SQL
                        await using var cmd = connection.CreateCommand();
                        cmd.Transaction = migrationTx;
                        cmd.CommandText = sqlContent;
                        await cmd.ExecuteNonQueryAsync();

                        // Step 3: Update to "success"
                        await txManager.CompleteMigrationAsync(recordId, "success");

                        // Commit transaction
                        if (migrationTx != null)
                            await migrationTx.CommitAsync();

                        Console.WriteLine($"✅ Applied:     {entry.Name}");
                        totalApplied++;
                    }
                    catch (Exception ex)
                    {
                        // Build error log
                        var errorLog = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] ERROR: {ex.Message}";
                        if (ex.InnerException != null)
                            errorLog += $"\n  → {ex.InnerException.Message}";

                        Console.Error.WriteLine($"❌ Failed:      {entry.Name} — {ex.Message}");

                        // Rollback transaction
                        if (migrationTx != null)
                        {
                            try { await migrationTx.RollbackAsync(); }
                            catch { /* best effort rollback */ }
                        }

                        // Record the failure (without transaction)
                        try
                        {
                            var failManager = new MigrationManager(
                                connection,
                                null, // no transaction for the failure record
                                options.MigrationSchema,
                                options.MigrationTable
                            );

                            // Check if we already inserted the running record via MigrationId
                            var existing = await failManager.GetMigrationsByMigrationIdAsync(entry.IncrementalId);
                            var running = existing.FirstOrDefault(e => e.Status == "running");

                            if (running.Id > 0)
                            {
                                await failManager.CompleteMigrationAsync(running.Id, "failed", errorLog);
                            }
                            else
                            {
                                // No running record found — insert a failed record
                                var failedId = await failManager.StartMigrationAsync(entry.IncrementalId, entry.Name, entry.Checksum);
                                await failManager.CompleteMigrationAsync(failedId, "failed", errorLog);
                            }
                        }
                        catch (Exception recordEx)
                        {
                            Console.Error.WriteLine($"⚠️  Could not record failure: {recordEx.Message}");
                        }

                        totalFailed++;
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                if (totalFailed == 0)
                {
                    Console.WriteLine($"✅ Successfully applied all {totalApplied} migration(s)!");
                }
                else
                {
                    Console.WriteLine($"⚠️  Applied {totalApplied}, Failed {totalFailed} migration(s).");
                    Console.WriteLine($"   Check {options.MigrationSchema}.{options.MigrationTable} table for error logs.");
                    return 1;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Resolves the output directory, substituting {provider} placeholder.
    /// </summary>
    private static string ResolveOutputDir(string outputDir, string provider)
    {
        var resolved = outputDir.Replace("{provider}", provider);
        return Path.GetFullPath(resolved);
    }

    /// <summary>
    /// Creates a DbConnection from the given assembly-qualified type name and connection string.
    /// </summary>
    private static DbConnection CreateConnection(string connectionTypeName, string connectionString)
    {
        var type = Type.GetType(connectionTypeName);
        if (type == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(connectionTypeName.Split(',')[0].Trim());
                if (type != null) break;
            }
        }

        if (type == null)
            throw new InvalidOperationException(
                $"Cannot find DbConnection type '{connectionTypeName}'. " +
                $"Ensure the appropriate ADO.NET provider NuGet package is installed.");

        var connection = (DbConnection)Activator.CreateInstance(type)!;
        connection.ConnectionString = connectionString;
        return connection;
    }
}

/// <summary>
/// Options for the 'apply' command.
/// </summary>
public class ApplyOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Database connection string.</summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>Output directory for migration files.</summary>
    public string OutputDir { get; set; } = "./Migrations/{provider}";

    /// <summary>Schema name for the migration tracking table.</summary>
    public string MigrationSchema { get; set; } = "public";

    /// <summary>Table name for the migration tracking table.</summary>
    public string MigrationTable { get; set; } = "__Migrations";
}

using System.Data.Common;
using Drizzle4Dotnet.Cli.Models;
using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'apply' command: applies pending SQL migration scripts
/// from the migration journal to a database using the CLI's MigrationManager.
/// Migrations are recorded only after successful execution, and detailed
/// execution logs (start, end, errors) are written to a shared local log file.
/// Wraps each migration in a transaction when possible.
/// Verifies checksums of already-applied migrations against the journal to detect tampering.
/// </summary>
public static class ApplyCommand
{
    /// <summary>
    /// Maps provider names to known DbConnection type names for automatic creation.
    /// Used to instantiate the correct ADO.NET connection without requiring a direct dependency
    /// on each provider's NuGet package at compile time.
    /// </summary>
    private static readonly Dictionary<string, string> ConnectionTypeMap = new()
    {
        ["pgsql"] = "Npgsql.NpgsqlConnection, Npgsql",
        ["mysql"] = "MySql.Data.MySqlClient.MySqlConnection, MySql.Data",
        ["mssql"] = "Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient",
        ["sqlite"] = "Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite",
        ["oracle"] = "Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess",
    };

    /// <summary>
    /// Executes the migration apply process: loads the migration journal, connects to the database,
    /// verifies checksums of already-applied migrations, determines pending migrations,
    /// and applies each one in order within individual transactions.
    /// </summary>
    /// <param name="options">Parsed CLI options including provider, connection string, migration table, etc.</param>
    /// <returns>Exit code: 0 on success, 1 on error.</returns>
    public static async Task<int> Execute(ApplyOptions options)
    {
        MigrationLogger? migrationLog = null;
        try
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            migrationLog = new MigrationLogger(logDir);
            
            var provider = options.Provider.ToLowerInvariant();
            if (!ConnectionTypeMap.TryGetValue(provider, out var connectionTypeName))
                throw new ArgumentException($"Unknown provider '{options.Provider}'. Supported providers: {string.Join(", ", ConnectionTypeMap.Keys)}");

            // Resolve migrations directory
            var outputDir = ResolveOutputDir(options.OutputDir, provider);
            var journalPath = Path.Combine(outputDir, "migration-journal.json");
            if (!File.Exists(journalPath))
                throw new FileNotFoundException($"Migration journal not found at '{journalPath}'. Ensure migrations have been generated before applying.");

            // Load journal
            var journal = MigrationJournal.Load(journalPath);
            if (journal.Migrations.Count == 0)
            {
                Console.WriteLine("  ✅ No migrations found in journal. Nothing to apply.");
                migrationLog.Log("COMPLETE", "No migrations found in journal. Nothing to apply.");
                return 0;
            }

            CliOptionParser.PrintBanner(
                "📋 Migration Journal Loaded",
                ("Provider:", options.Provider),
                ("Total:", $"{journal.Migrations.Count} migration(s)"),
                ("Directory:", Path.GetFullPath(outputDir)),
                ("Log:", migrationLog.LogFilePath),
                ("Tracking:", $"{options.MigrationSchema}.{options.MigrationTable}")
            );

            CliOptionParser.LogBanner(migrationLog, "Migration Apply",
                ("PROVIDER", options.Provider),
                ("JOURNAL", $"loaded from {journalPath}"),
                ("MIGRATIONS", $"{journal.Migrations.Count} migration(s)"),
                ("TRACKING", $"{options.MigrationSchema}.{options.MigrationTable}")
            );

            // Create database connection
            Console.WriteLine($"  🔌 Connecting to database...");

            DbConnection connection;
            try
            {
                connection = CreateConnection(connectionTypeName, options.ConnectionString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create database connection for provider '{options.Provider}': {ex.Message}", ex);
            }

            await using (connection)
            {
                await connection.OpenAsync();
                Console.WriteLine($"  ✅ Connected to database");
                migrationLog.Log("CONNECTED", "Connected to database");

                // Create the CLI MigrationManager directly — no generics, no dynamic
                var manager = new MigrationManager(
                    connection,
                    null, // no external transaction — we manage our own per migration
                    options.MigrationSchema,
                    options.MigrationTable
                );

                // Get already applied migrations from database
                Console.WriteLine($"  🔍 Checking applied migrations...");
                var appliedMigrations = await manager.GetAppliedMigrationsAsync();

                Console.WriteLine($"  Applied:      {appliedMigrations.Count} migration(s) in database");
                migrationLog.Log("APPLIED", $"{appliedMigrations.Count} migration(s) already applied in database");

                // Verify checksums of already-applied migrations against the journal
                var checksumErrors = false;
                foreach (var entry in journal.Migrations)
                {
                    var applied = appliedMigrations.FirstOrDefault(a => a.MigrationId == entry.IncrementalId);
                    if (applied.Id == 0) continue;

                    // Compare the database checksum with the journal checksum
                    if (!string.Equals(applied.Checksum, entry.Checksum, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine($"  Checksum mismatch for applied migration #{entry.IncrementalId} ({entry.Name})");
                        Console.Error.WriteLine($"    Database:  {applied.Checksum}");
                        Console.Error.WriteLine($"    Journal:   {entry.Checksum}");
                        Console.Error.WriteLine("    The migration may have been tampered with after being applied.");
                        migrationLog.Log("CHECKSUM", $"Mismatch for #{entry.IncrementalId} ({entry.Name}): DB={applied.Checksum}, Journal={entry.Checksum}");
                        checksumErrors = true;
                    }
                }
                
                if (checksumErrors)
                {
                    throw new InvalidOperationException("Checksum mismatch detected for one or more applied migrations. Aborting to prevent potential data corruption.");
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
                        Console.Error.WriteLine($"  ⚠  SQL file not found: {entry.SqlFileName} — skipping");
                        migrationLog.Log("WARNING", $"SQL file not found: {entry.SqlFileName} — skipping");
                        continue;
                    }

                    var sqlContent = await File.ReadAllTextAsync(sqlPath);
                    pending.Add((entry, sqlContent));
                }

                if (pending.Count == 0)
                {
                    Console.WriteLine($"  ✅ All {journal.Migrations.Count} migration(s) already applied. Database is up to date.");
                    migrationLog.Log("COMPLETE", $"All {journal.Migrations.Count} migration(s) already applied. Database is up to date.");
                    return 0;
                }

                Console.WriteLine($"  Pending:      {pending.Count} migration(s) to apply");
                CliOptionParser.PrintSeparator();
                migrationLog.Log("PENDING", $"{pending.Count} migration(s) to apply");

                // Apply pending migrations in order
                var totalApplied = 0;
                foreach (var (entry, sqlContent) in pending)
                {
                    Console.WriteLine();
                    Console.WriteLine($"  ⚡ Applying:    {entry.Name} ({entry.SqlFileName})");

                    migrationLog.LogSeparator();
                    migrationLog.Log("EXECUTE", $"Applying migration #{entry.IncrementalId}: '{entry.Name}' ({entry.SqlFileName}), CHECKSUM={entry.Checksum}");

                    // Try to begin a transaction for this migration
                    DbTransaction? migrationTx = null;
                    try
                    {
                        migrationTx = await connection.BeginTransactionAsync();
                    }
                    catch
                    {
                        Console.WriteLine("  ⚠  Transaction not supported, running without transaction");
                        migrationLog.Log("WARNING", "Transaction not supported, running without transaction");
                    }

                    try
                    {
                        // Execute the raw SQL
                        await using var cmd = connection.CreateCommand();
                        cmd.Transaction = migrationTx;
                        cmd.CommandText = sqlContent;
                        await cmd.ExecuteNonQueryAsync();

                        // Commit transaction first (so the migration record insert is outside the migration's transaction)
                        if (migrationTx != null)
                            await migrationTx.CommitAsync();

                        // Only insert record on success
                        var txManager = new MigrationManager(
                            connection,
                            null, // no transaction for recording (already committed)
                            options.MigrationSchema,
                            options.MigrationTable
                        );
                        
                        await txManager.InsertMigrationAsync(
                            entry.IncrementalId,
                            entry.Name,
                            entry.Checksum
                        );
                        totalApplied++;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"  Failed:      {entry.IncrementalId}: {entry.Name} — {ex.Message}");

                        // Rollback transaction
                        if (migrationTx != null)
                        {
                            try { await migrationTx.RollbackAsync(); }
                            catch { /* best effort rollback */ }
                        }
                        
                        migrationLog.Log("FAILED", $"Migration #{entry.IncrementalId} '{entry.Name}' failed: {ex.Message}");
                        migrationLog.LogSqlError(sqlContent, ex);
                        
                        throw new InvalidOperationException($"Migration #{entry.IncrementalId} '{entry.Name}' failed. See log at '{migrationLog.LogFilePath}' for details.", ex);
                    }
                    
                    migrationLog.Log("SUCCESS", $"Migration #{entry.IncrementalId} '{entry.Name}' applied successfully.");
                    Console.WriteLine($"  ✅ Applied:     {entry.IncrementalId}: {entry.Name}");
                }

                migrationLog.LogSeparator();
                Console.WriteLine();
                CliOptionParser.PrintSeparator();
                Console.WriteLine($"  ✅ Successfully applied all {totalApplied} migration(s)!");
                
                migrationLog.Log("COMPLETE", $"Successfully applied all {totalApplied} migration(s)!");
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  Error: {ex.Message}");
            if (options.Verbose)
                Console.Error.WriteLine($"  {ex.StackTrace}");
            migrationLog?.LogError(ex);
            return 1;
        }
        finally
        {
            if (migrationLog != null)
                await migrationLog.DisposeAsync();
        }
    }

    /// <summary>
    /// Resolves the output directory path, substituting the {provider} placeholder with the actual provider name.
    /// </summary>
    /// <param name="outputDir">Output directory template (may contain {provider}).</param>
    /// <param name="provider">The provider name to substitute (e.g., "pgsql", "mysql").</param>
    /// <returns>Fully qualified output directory path.</returns>
    private static string ResolveOutputDir(string outputDir, string provider)
    {
        var resolved = outputDir.Replace("{provider}", provider);
        return Path.GetFullPath(resolved);
    }

    /// <summary>
    /// Creates a DbConnection from the given assembly-qualified type name and connection string.
    /// Attempts to resolve the type via Type.GetType first, then falls back to scanning loaded assemblies.
    /// </summary>
    /// <param name="connectionTypeName">Assembly-qualified type name (e.g., "Npgsql.NpgsqlConnection, Npgsql").</param>
    /// <param name="connectionString">Database connection string.</param>
    /// <returns>An initialized DbConnection instance with the connection string set.</returns>
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

    /// <summary>Enable verbose output with detailed debug information.</summary>
    public bool Verbose { get; set; }
}

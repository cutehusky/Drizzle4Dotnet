using System.Data.Common;
using Drizzle4Dotnet.Cli.Models;
using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'status' command: displays the migration journal and current state,
/// and optionally checks database migration status if a connection string is provided.
/// </summary>
public static class StatusCommand
{
    /// <summary>
    /// Maps provider names to known DbConnection type names for automatic creation.
    /// Used to instantiate the correct ADO.NET connection for checking database migration status.
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
    /// Executes the status display: loads the migration journal from the output directory,
    /// displays all tracked migrations with their metadata, and optionally connects to the
    /// database to show applied vs. pending migration status and checksum verification.
    /// </summary>
    /// <param name="options">Parsed CLI options including output directory, provider, connection string, etc.</param>
    /// <returns>Exit code: 0 on success, 1 on error.</returns>
    public static async Task<int> Execute(StatusOptions options)
    {
        try
        {
            var outputDir = Path.GetFullPath(options.OutputDir);
            var journalPath = Path.Combine(outputDir, "migration-journal.json");
            Console.WriteLine($"  📋 Migration Status");
            CliOptionParser.PrintSeparator();

            // --- Journal status ---
            if (!File.Exists(journalPath))
            {
                Console.WriteLine($"  No migration journal found at:");
                Console.WriteLine($"  {journalPath}");
                Console.WriteLine();
                Console.WriteLine($"  Use 'drizzle4net generate' to create your first migration.");
            }
            else
            {
                var journal = MigrationJournal.Load(journalPath);

                Console.WriteLine($"  Provider:     {journal.Provider}");
                Console.WriteLine($"  Journal:      {journalPath}");
                Console.WriteLine($"  Migrations:   {journal.Migrations.Count} total");
                Console.WriteLine();
                
                if (journal.Migrations.Count == 0)
                {
                    Console.WriteLine("  (no migrations recorded)");
                }
                else
                {
                    for (var i = 0; i < journal.Migrations.Count; i++)
                    {
                        var entry = journal.Migrations[i];
                        Console.WriteLine($"  #{i + 1}: {entry.Name}");
                        Console.WriteLine($"      ID:         {entry.IncrementalId}");
                        Console.WriteLine($"      Generated:  {entry.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                        Console.WriteLine($"      SQL Up File:   {entry.SqlFileName}");
                        Console.WriteLine($"      SQL Down File:  {entry.DownSqlFileName}");
                        Console.WriteLine($"      Snapshot:   {entry.SnapshotFileName}");
                        Console.WriteLine($"      Checksum:   {entry.Checksum[..Math.Min(16, entry.Checksum.Length)]}...");
                        Console.WriteLine($"      Desc:       {entry.Description}");
                    }
                }

                CliOptionParser.PrintSeparator();

                // --- Database status (if connection string provided) ---
                if (!string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    Console.WriteLine();
                    Console.WriteLine($"  🔌 Checking database status...");

                    // Resolve provider connection type
                    var provider = options.Provider.ToLowerInvariant();
                    if (!ConnectionTypeMap.TryGetValue(provider, out var connectionTypeName))
                    {
                        Console.Error.WriteLine($"  ⚠ Unknown provider '{options.Provider}'.");
                        Console.Error.WriteLine($"    Supported providers: {string.Join(", ", ConnectionTypeMap.Keys)}");
                    }
                    else
                    {
                        DbConnection? connection = null;
                        try
                        {
                            connection = CreateConnection(connectionTypeName, options.ConnectionString);
                            await connection.OpenAsync();

                            var manager = new MigrationManager(
                                connection,
                                null,
                                options.MigrationSchema,
                                options.MigrationTable
                            );

                            var appliedMigrations = await manager.GetAppliedMigrationsAsync();

                            Console.WriteLine($"  Table:        {options.MigrationSchema}.{options.MigrationTable}");
                            Console.WriteLine($"  Applied:      {appliedMigrations.Count} migration(s) in database");
                            Console.WriteLine();
                            
                            // Show applied migrations
                            if (appliedMigrations.Count > 0)
                            {
                                Console.WriteLine($"  Applied Migrations:");
                                foreach (var applied in appliedMigrations)
                                {
                                    var journalEntry = journal.Migrations.FirstOrDefault(
                                        e => e.IncrementalId == applied.MigrationId);

                                    Console.WriteLine($"    ✅ #{applied.MigrationId}: {applied.Name}");
                                    Console.WriteLine($"        AppliedAt: {applied.AppliedAt:yyyy-MM-dd HH:mm:ss}");
                                    Console.WriteLine($"        Checksum:  {applied.Checksum[..Math.Min(16, applied.Checksum.Length)]}...");

                                    if (journalEntry != null)
                                    {
                                        var match = string.Equals(applied.Checksum, journalEntry.Checksum, StringComparison.OrdinalIgnoreCase);
                                        Console.WriteLine($"        Journal:   {(match ? "✅ matches" : "❌ checksum mismatch!")} DB: {applied.Checksum[..Math.Min(16, applied.Checksum.Length)]}... vs Journal: {journalEntry.Checksum[..Math.Min(16, journalEntry.Checksum.Length)]}...");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"        Journal:   ⚠ not found in journal (may have been removed)");
                                    }
                                }
                            }

                            // Show pending migrations (in journal but not in DB)
                            var pending = journal.Migrations
                                .Where(j => !appliedMigrations.Any(a => a.MigrationId == j.IncrementalId))
                                .ToList();

                            if (pending.Count > 0)
                            {
                                Console.WriteLine();
                                Console.WriteLine($"  Pending Migrations:");
                                foreach (var entry in pending)
                                {
                                    Console.WriteLine($"    ⏳ #{entry.IncrementalId}: {entry.Name}");
                                    Console.WriteLine($"        Generated: {entry.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                                    Console.WriteLine($"        SQL File:  {entry.SqlFileName}");
                                }
                            }

                            // Check for extra migrations in DB not in journal
                            var extra = appliedMigrations
                                .Where(a => !journal.Migrations.Any(j => j.IncrementalId == a.MigrationId))
                                .ToList();

                            if (extra.Count > 0)
                            {
                                Console.WriteLine();
                                Console.WriteLine($"  ⚠ Orphaned DB Records (not in journal):");
                                foreach (var applied in extra)
                                {
                                    Console.WriteLine($"    🗄️ #{applied.MigrationId}: {applied.Name} (applied {applied.AppliedAt:yyyy-MM-dd HH:mm:ss})");
                                }
                            }
                        }
                        catch (Exception dbEx)
                        {
                            Console.Error.WriteLine($"  ⚠ Could not check database status: {dbEx.Message}");
                            if (options.Verbose)
                                Console.Error.WriteLine(dbEx.StackTrace);
                        }
                        finally
                        {
                            if (connection != null)
                                await connection.DisposeAsync();
                        }
                    }
                }
            }

            CliOptionParser.PrintSeparator();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  Error: {ex.Message}");
            if (options.Verbose)
                Console.Error.WriteLine($"  {ex.StackTrace}");
            return 1;
        }
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
/// Options for the 'status' command.
/// </summary>
public class StatusOptions
{
    /// <summary>Output directory for migration files.</summary>
    public string OutputDir { get; set; } = "./Migrations/{provider}";

    /// <summary>Database provider.</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Database connection string (optional). If provided, also checks DB migration status.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Schema name for the migration tracking table.</summary>
    public string MigrationSchema { get; set; } = "public";

    /// <summary>Table name for the migration tracking table.</summary>
    public string MigrationTable { get; set; } = "__Migrations";

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}

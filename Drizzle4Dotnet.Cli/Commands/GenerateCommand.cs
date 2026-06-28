using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'generate' command: generates a new SQL migration script
/// and updates the migration journal JSON.
/// </summary>
public static class GenerateCommand
{
    public static int Execute(GenerateOptions options)
    {
        try
        {
            var provider = ParseProvider(options.Provider);
            var generator = new MigrationGenerator(provider);

            Console.WriteLine($"📦 Drizzle4Dotnet Migration Generator");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {generator.ProviderName}");
            Console.WriteLine($"  Migration:    {options.MigrationName}");
            Console.WriteLine($"  Output:       {Path.GetFullPath(options.OutputDir)}");
            Console.WriteLine($"  Tables:       {options.TableTypes.Count} type(s)");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Resolve the output directory and ensure it exists
            var outputDir = ResolveOutputDirectory(options.OutputDir, generator.ProviderName);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Find the latest snapshot
            var snapshotPath = options.SnapshotPath;
            if (snapshotPath == null)
            {
                // Auto-discover the latest snapshot
                var snapshots = Directory.GetFiles(outputDir, "*-snapshot-*.json")
                    .OrderByDescending(f => f)
                    .ToArray();

                if (snapshots.Length > 0)
                {
                    snapshotPath = snapshots[0];
                    Console.WriteLine($"  Snapshot:     {Path.GetFileName(snapshotPath)} (auto-discovered)");
                }
                else
                {
                    Console.WriteLine($"  Snapshot:     None (initial migration)");
                }
            }
            else
            {
                Console.WriteLine($"  Snapshot:     {snapshotPath}");
            }

            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Print detailed schema info when verbose
            if (options.Verbose)
            {
                var snapshot = generator.GenerateSnapshot(
                    options.MigrationName, options.TableTypes, options.AssemblyPath);
                SchemaDebugPrinter.PrintSchema("📊 Schema Details (verbose)", snapshot);
            }

            // Generate the migration
            var result = generator.GenerateMigration(
                options.MigrationName,
                options.TableTypes,
                options.AssemblyPath,
                outputDir,
                snapshotPath
            );

            Console.WriteLine();

            if (string.IsNullOrEmpty(result.SqlFilePath))
            {
                Console.WriteLine($"⏭️  No schema changes detected. Nothing to generate.");
                Console.WriteLine();
                Console.WriteLine($"  Description:  {result.Description}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"✅ Migration generated successfully!");
                Console.WriteLine();
                Console.WriteLine($"  SQL Script:   {result.SqlFilePath}");
                Console.WriteLine($"  Snapshot:     {result.SnapshotFilePath}");
                Console.WriteLine($"  Checksum:     {result.Checksum}");
                Console.WriteLine();
                Console.WriteLine($"  Description:");
                Console.WriteLine($"    {result.Description}");
                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            if (options.Verbose)
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static DatabaseProvider ParseProvider(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "pgsql" or "postgres" or "postgresql" or "npgsql" => DatabaseProvider.PgSql,
            "mysql" or "mariadb" => DatabaseProvider.MySql,
            "mssql" or "sqlserver" or "sql-server" => DatabaseProvider.Mssql,
            "sqlite" or "sqlite3" => DatabaseProvider.Sqlite,
            "oracle" => DatabaseProvider.Oracle,
            _ => throw new ArgumentException(
                $"Unknown provider '{provider}'. Supported providers: pgsql, mysql, mssql, sqlite, oracle")
        };
    }

    private static string ResolveOutputDirectory(string outputDir, string providerName)
    {
        // If the output dir contains {provider}, substitute it
        var resolved = outputDir.Replace("{provider}", providerName);
        return Path.GetFullPath(resolved);
    }
}

/// <summary>
/// Options for the 'generate' command.
/// </summary>
public class GenerateOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Name for the migration (e.g., "v1.0.0", "add_users_table").</summary>
    public string MigrationName { get; set; } = "";

    /// <summary>Fully qualified type names of ORM table classes.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Output directory for migration files.</summary>
    public string OutputDir { get; set; } = "./Migrations/{provider}";

    /// <summary>Path to the current snapshot JSON file.</summary>
    public string? SnapshotPath { get; set; }

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}

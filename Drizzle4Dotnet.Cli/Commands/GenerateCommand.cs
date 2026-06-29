using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'generate' command: generates a new SQL migration script
/// and updates the migration journal JSON.
/// Creates .sql files (up/down), snapshot JSON, and updates migration-journal.json.
/// </summary>
public static class GenerateCommand
{
    /// <summary>
    /// Executes the migration generation process: creates a schema snapshot from ORM table types,
    /// diffs against the previous snapshot, generates SQL migration scripts, and updates the journal.
    /// </summary>
    /// <param name="options">Parsed CLI options including provider, migration name, table types, etc.</param>
    /// <returns>Exit code: 0 on success, 1 on error.</returns>
    public static int Execute(GenerateOptions options)
    {
        MigrationLogger? migrationLog = null;
        try
        {
            // Create file logger for this generation
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            migrationLog = new MigrationLogger(logDir);

            var provider = MigrationGenerator.ParseCliOptionProvider(options.Provider);
            var generator = new MigrationGenerator(provider)
            {
                Verbose = options.Verbose,
                AutoCreateSchema = !options.NoAutoCreateSchema
            };

            // Resolve the output directory and ensure it exists
            var outputDir = ResolveOutputDirectory(options.OutputDir, generator.ProviderName);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Standardized banner
            CliOptionParser.PrintBanner(
                "📦 Drizzle4Dotnet Migration Generator",
                ("Provider:", generator.ProviderName),
                ("Migration:", options.MigrationName),
                ("Output:", Path.GetFullPath(outputDir)),
                ("Tables:", $"{options.TableTypes.Count} type(s)"),
                ("Log:", Path.GetFullPath(logDir)),
                ("Auto Schema:", generator.AutoCreateSchema ? "Yes" : "No")
            );

            CliOptionParser.LogBanner(migrationLog, "Migration Generator",
                ("PROVIDER", generator.ProviderName),
                ("MIGRATION", options.MigrationName),
                ("TABLES", options.TableTypes.Count > 0 ? string.Join(", ", options.TableTypes) : "auto-discover")
            );

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
                    migrationLog.Log("SNAPSHOT", $"Using snapshot: {snapshotPath} (auto-discovered)");
                }
                else
                {
                    Console.WriteLine($"  Snapshot:     None (initial migration)");
                    migrationLog.Log("SNAPSHOT", "No existing snapshot found (initial migration)");
                }
            }
            else
            {
                Console.WriteLine($"  Snapshot:     {snapshotPath}");
                migrationLog.Log("SNAPSHOT", $"Using snapshot: {snapshotPath}");
            }

            CliOptionParser.PrintSeparator();

            // Print detailed schema info when verbose
            if (options.Verbose)
            {
                var snapshot = generator.GenerateSnapshot(
                    options.MigrationName, options.TableTypes, options.AssemblyPath);
                SchemaDebugPrinter.PrintSchema("📊 Schema Details (verbose)", snapshot);
            }

            migrationLog.Log("GENERATE", "Starting migration generation...");

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
                Console.WriteLine("  ⏭️  No schema changes detected. Nothing to generate.");
                Console.WriteLine();
                Console.WriteLine($"  Description:  {result.Description}");
                Console.WriteLine();
                migrationLog.Log("CHANGES", $"No schema changes detected. {result.Description}");
            }
            else
            {
                Console.WriteLine("  ✅ Migration generated successfully!");
                Console.WriteLine();
                Console.WriteLine($"  Up SQL:       {result.SqlFilePath}");
                Console.WriteLine($"  Down SQL:     {result.DownSqlFilePath}");
                Console.WriteLine($"  Snapshot:     {result.SnapshotFilePath}");
                Console.WriteLine($"  Checksum:     {result.Checksum}");
                Console.WriteLine();
                Console.WriteLine($"  Description:");
                Console.WriteLine($"    {result.Description}");
                Console.WriteLine();

                migrationLog.Log("SUCCESS", "Migration generated successfully");
                migrationLog.Log("OUTPUT", $"Up SQL: {result.SqlFilePath}");
                migrationLog.Log("OUTPUT", $"Down SQL: {result.DownSqlFilePath}");
                migrationLog.Log("OUTPUT", $"Snapshot: {result.SnapshotFilePath}");
                migrationLog.Log("CHECKSUM", result.Checksum);
                migrationLog.Log("DESCRIPTION", result.Description ?? "");
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
            migrationLog?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Resolves the output directory path, substituting the {provider} placeholder with the actual provider name.
    /// </summary>
    /// <param name="outputDir">Output directory template (may contain {provider}).</param>
    /// <param name="providerName">The provider name to substitute.</param>
    /// <returns>Fully qualified output directory path.</returns>
    private static string ResolveOutputDirectory(string outputDir, string providerName)
    {
        var resolved = outputDir.Replace("{provider}", providerName);
        return Path.GetFullPath(resolved);
    }
}

/// <summary>
/// Options for the 'generate' command, parsed from CLI arguments.
/// </summary>
public class GenerateOptions : IAssemblyOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Name for the migration (e.g., "v1.0.0", "add_users_table").</summary>
    public string MigrationName { get; set; } = "";

    /// <summary>Fully qualified type names of ORM table classes to include in the migration.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Output directory for migration files. Use {provider} placeholder for provider name substitution.</summary>
    public string OutputDir { get; set; } = "./Migrations/{provider}";

    /// <summary>Path to the current snapshot JSON file for diff comparison.</summary>
    public string? SnapshotPath { get; set; }

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>
    /// If true, disables automatic schema/database creation in the generated migration SQL.
    /// Default: false (schema/database creation IS auto-generated).
    /// </summary>
    public bool NoAutoCreateSchema { get; set; }

    /// <summary>Enable verbose output with detailed debug information.</summary>
    public bool Verbose { get; set; }
}

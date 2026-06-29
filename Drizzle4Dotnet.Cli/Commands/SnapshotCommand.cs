using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'snapshot' command: generates only a schema snapshot JSON
/// from ORM table types without creating a migration SQL script.
/// Useful for capturing the current state of the schema at a point in time.
/// </summary>
public static class SnapshotCommand
{
    /// <summary>
    /// Executes the snapshot generation process: extracts table definitions from ORM types,
    /// serializes to a schema snapshot JSON file, and logs the results.
    /// Prints detailed schema information when verbose mode is enabled.
    /// </summary>
    /// <param name="options">Parsed CLI options including provider, snapshot name, table types, etc.</param>
    /// <returns>Exit code: 0 on success, 1 on error.</returns>
    public static int Execute(SnapshotOptions options)
    {
        MigrationLogger? migrationLog = null;
        try
        {
            // Create file logger in the common logs folder
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            migrationLog = new MigrationLogger(logDir);
            
            var outputFile = Path.GetFullPath(options.OutputFile);
            var outputDir = Path.GetDirectoryName(outputFile) ?? ".";
            
            var provider = MigrationGenerator.ParseCliOptionProvider(options.Provider);
            var generator = new MigrationGenerator(provider)
            {
                Verbose = options.Verbose
            };

            // Standardized banner
            CliOptionParser.PrintBanner(
                "📸 Drizzle4Dotnet Schema Snapshot Generator",
                ("Provider:", generator.ProviderName),
                ("Snapshot:", options.SnapshotName),
                ("Output:", outputFile),
                ("Tables:", $"{options.TableTypes.Count} type(s)"),
                ("Log:", Path.GetFullPath(logDir))
            );

            CliOptionParser.LogBanner(migrationLog, "Schema Snapshot Generator",
                ("PROVIDER", generator.ProviderName),
                ("SNAPSHOT", options.SnapshotName),
                ("OUTPUT", outputFile),
                ("TABLES", options.TableTypes.Count > 0 ? string.Join(", ", options.TableTypes) : "auto-discover")
            );

            migrationLog.Log("GENERATE", "Starting snapshot generation...");

            // Generate the snapshot (extracts table definitions)
            var snapshot = generator.GenerateSnapshot(
                options.SnapshotName,
                options.TableTypes,
                options.AssemblyPath
            );

            migrationLog.Log("TABLES", $"Extracted {snapshot.Tables.Count} table(s) from schema");

            // Print detailed schema info when verbose
            if (options.Verbose)
                SchemaDebugPrinter.PrintSchema("📊 Schema Details (verbose)", snapshot);

            // Serialize and write
            var json = snapshot.Serialize();

            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            File.WriteAllText(outputFile, json);

            Console.WriteLine();
            Console.WriteLine("  ✅ Snapshot generated successfully!");
            Console.WriteLine();
            Console.WriteLine($"  File:         {outputFile}");
            Console.WriteLine($"  Name:         {snapshot.Name}");
            Console.WriteLine($"  Tables:       {snapshot.Tables.Count}");
            Console.WriteLine();

            migrationLog.Log("SUCCESS", $"Snapshot generated successfully");
            migrationLog.Log("OUTPUT", outputFile);
            migrationLog.Log("TABLES", snapshot.Tables.Count.ToString());

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
}

/// <summary>
/// Options for the 'snapshot' command, parsed from CLI arguments.
/// </summary>
public class SnapshotOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Name for the snapshot (e.g., "v1.0.0", "initial_schema").</summary>
    public string SnapshotName { get; set; } = "";

    /// <summary>Fully qualified type names of ORM table classes to include in the snapshot.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Output file path for the snapshot JSON.</summary>
    public string OutputFile { get; set; } = "./snapshot.json";

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>Enable verbose output with detailed debug information.</summary>
    public bool Verbose { get; set; }
}

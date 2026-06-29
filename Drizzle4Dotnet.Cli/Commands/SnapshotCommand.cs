using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'snapshot' command: generates only a schema snapshot JSON
/// from ORM table types without creating a migration SQL script.
/// </summary>
public static class SnapshotCommand
{
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
            var generator = new MigrationGenerator(provider);

            Console.WriteLine($"📸 Drizzle4Dotnet Schema Snapshot Generator");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {generator.ProviderName}");
            Console.WriteLine($"  Snapshot:     {options.SnapshotName}");
            Console.WriteLine($"  Output:       {outputFile}");
            Console.WriteLine($"  Tables:       {options.TableTypes.Count} type(s)");
            Console.WriteLine($"  Log:          {Path.GetFullPath(logDir)}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            migrationLog.Log("PROVIDER", $"Provider: {generator.ProviderName}");
            migrationLog.Log("SNAPSHOT", $"Snapshot name: {options.SnapshotName}");
            migrationLog.Log("OUTPUT", $"Output file: {outputFile}");
            migrationLog.Log("TABLES", $"Table types: {(options.TableTypes.Count > 0 ? string.Join(", ", options.TableTypes) : "auto-discover")}");
            migrationLog.LogSeparator();

            migrationLog.Log("GENERATE", "Starting snapshot generation...");

            // Generate the snapshot (extracts table definitions)
            var snapshot = generator.GenerateSnapshot(
                options.SnapshotName,
                options.TableTypes,
                options.AssemblyPath
            );

            migrationLog.Log("TABLES-COUNT", $"Extracted {snapshot.Tables.Count} table(s) from schema");

            // Print detailed schema info when verbose
            if (options.Verbose)
                SchemaDebugPrinter.PrintSchema("📊 Schema Details (verbose)", snapshot);

            // Serialize and write
            var json = snapshot.Serialize();

            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            File.WriteAllText(outputFile, json);

            Console.WriteLine();
            Console.WriteLine($"✅ Snapshot generated successfully!");
            Console.WriteLine();
            Console.WriteLine($"  File:         {outputFile}");
            Console.WriteLine($"  Name:         {snapshot.Name}");
            Console.WriteLine($"  Tables:       {snapshot.Tables.Count}");
            Console.WriteLine();

            migrationLog.Log("SUCCESS", $"Snapshot generated successfully");
            migrationLog.Log("FILE", outputFile);
            migrationLog.Log("TABLES-COUNT", snapshot.Tables.Count.ToString());

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            if (options.Verbose)
                Console.Error.WriteLine(ex.StackTrace);
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
/// Options for the 'snapshot' command.
/// </summary>
public class SnapshotOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Name for the snapshot (e.g., "v1.0.0").</summary>
    public string SnapshotName { get; set; } = "";

    /// <summary>Fully qualified type names of ORM table classes.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Output file path for the snapshot JSON.</summary>
    public string OutputFile { get; set; } = "./snapshot.json";

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}

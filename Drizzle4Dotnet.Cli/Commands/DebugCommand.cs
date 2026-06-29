using Drizzle4Dotnet.Cli.Services;
namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'debug' command: displays detailed schema information
/// (tables, columns, data types, attributes) from ORM table types.
/// Useful for inspecting the compiled table schema and diagnosing issues.
/// </summary>
public static class DebugCommand
{
    /// <summary>
    /// Executes the debug schema inspection: extracts table definitions from ORM types,
    /// generates a snapshot, and prints detailed schema information to the console.
    /// </summary>
    /// <param name="options">Parsed CLI options including provider, table types, assembly path, etc.</param>
    /// <returns>Exit code: 0 on success, 1 on error.</returns>
    public static int Execute(DebugOptions options)
    {
        try
        {
            var provider = MigrationGenerator.ParseCliOptionProvider(options.Provider);
            var generator = new MigrationGenerator(provider)
            {
                Verbose = options.Verbose
            };

            CliOptionParser.PrintBanner(
                "🔍 Drizzle4Dotnet Schema Debug",
                ("Provider:", generator.ProviderName),
                ("Tables:", $"{options.TableTypes.Count} type(s)")
            );

            // Generate the snapshot (which extracts all table definitions)
            var snapshot = generator.GenerateSnapshot(
                "debug", options.TableTypes, options.AssemblyPath);

            Console.WriteLine();
            SchemaDebugPrinter.PrintSchema("📊 Schema Summary", snapshot);

            CliOptionParser.PrintSeparator();
            Console.WriteLine($"  Total Tables: {snapshot.Tables.Count}");
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
}

/// <summary>
/// Options for the 'debug' command, parsed from CLI arguments.
/// </summary>
public class DebugOptions : IAssemblyOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Fully qualified type names of ORM table classes to inspect.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>Enable verbose output with detailed debug information.</summary>
    public bool Verbose { get; set; }
}

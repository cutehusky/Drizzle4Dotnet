using Drizzle4Dotnet.Cli.Services;
namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'debug' command: displays detailed schema information
/// (tables, columns, data types, attributes) from ORM table types.
/// </summary>
public static class DebugCommand
{
    public static int Execute(DebugOptions options)
    {
        try
        {
            var provider = MigrationGenerator.ParseCliOptionProvider(options.Provider);
            var generator = new MigrationGenerator(provider);

            Console.WriteLine($"🔍 Drizzle4Dotnet Schema Debug");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {generator.ProviderName}");
            Console.WriteLine($"  Tables:       {options.TableTypes.Count} type(s)");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Generate the snapshot (which extracts all table definitions)
            var snapshot = generator.GenerateSnapshot(
                "debug", options.TableTypes, options.AssemblyPath);

            Console.WriteLine();
            SchemaDebugPrinter.PrintSchema("📊 Schema Summary", snapshot);

            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Total Tables: {snapshot.Tables.Count}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

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
}

/// <summary>
/// Options for the 'debug' command.
/// </summary>
public class DebugOptions
{
    /// <summary>The database provider (pgsql, mysql, mssql, sqlite, oracle).</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Fully qualified type names of ORM table classes.</summary>
    public List<string> TableTypes { get; set; } = new();

    /// <summary>Path to the assembly containing the table types.</summary>
    public string? AssemblyPath { get; set; }

    /// <summary>Path to the .csproj file. If set, the project will be built and AssemblyPath resolved automatically.</summary>
    public string? ProjectPath { get; set; }

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}

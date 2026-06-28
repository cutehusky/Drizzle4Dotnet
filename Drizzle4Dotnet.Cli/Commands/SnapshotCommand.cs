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
        try
        {
            var provider = ParseProvider(options.Provider);
            var generator = new MigrationGenerator(provider);

            Console.WriteLine($"📸 Drizzle4Dotnet Schema Snapshot Generator");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {generator.ProviderName}");
            Console.WriteLine($"  Snapshot:     {options.SnapshotName}");
            Console.WriteLine($"  Output:       {Path.GetFullPath(options.OutputFile)}");
            Console.WriteLine($"  Tables:       {options.TableTypes.Count} type(s)");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Generate the snapshot
            var snapshot = generator.GenerateSnapshot(
                options.SnapshotName,
                options.TableTypes,
                options.AssemblyPath
            );

            // Serialize and write
            var json = snapshot.Serialize();

            var outputFile = Path.GetFullPath(options.OutputFile);
            var outputDir = Path.GetDirectoryName(outputFile);
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

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}

using Drizzle4Dotnet.Cli.Services;
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Shared helper to print detailed schema information (used by debug command
/// and by generate/snapshot with --verbose).
/// </summary>
public static class SchemaDebugPrinter
{
    public static void PrintSchema(string label, SchemaSnapshot snapshot)
    {
        Console.WriteLine();
        Console.WriteLine($"  {label}");
        Console.WriteLine($"  Tables:       {snapshot.Tables.Count}");
        Console.WriteLine();

        foreach (var table in snapshot.Tables)
        {
            Console.WriteLine($"  📋 Table: {table.SchemaName}.{table.TableName}");
            Console.WriteLine($"     Columns:      {table.Columns.Count}");
            Console.WriteLine();

            if (table.Columns.Count > 0)
            {
                const int nameWidth = -28;
                const int typeWidth = -28;
                const int flagsWidth = -20;

                Console.WriteLine($"    {"Column",nameWidth} {"Type",typeWidth} {"Attributes",flagsWidth}");
                Console.WriteLine($"    {"──────",nameWidth} {"────",typeWidth} {"──────────",flagsWidth}");

                foreach (var col in table.Columns)
                {
                    var attrs = new List<string>();
                    if (col.IsPrimaryKey) attrs.Add("PK");
                    if (col.IsAutoIncrement) attrs.Add("AUTO_INCREMENT");
                    if (!col.IsNullable) attrs.Add("NOT NULL");
                    if (col.DefaultValue != null) attrs.Add($"DEFAULT={col.DefaultValue}");
                    if (col.CheckExpression != null) attrs.Add($"CHECK={col.CheckExpression}");

                    var attrStr = attrs.Count > 0 ? string.Join(", ", attrs) : "";
                    Console.WriteLine($"    {col.Name,nameWidth} {col.RawDataType,typeWidth} {attrStr,flagsWidth}");
                }
            }

            Console.WriteLine();
        }
    }
}

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
            var provider = ParseProvider(options.Provider);
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

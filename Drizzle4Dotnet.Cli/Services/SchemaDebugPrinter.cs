
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Cli.Services;

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
            
            // Print constraints
            if (table.ConstraintDefinitions.Count > 0)
            {
                Console.WriteLine($"     Constraints:  {table.ConstraintDefinitions.Count}");
                foreach (var constraint in table.ConstraintDefinitions)
                {
                    Console.WriteLine($"       {constraint}");
                }
                Console.WriteLine();
            }

            // Print indexes
            if (table.IndexDefinitions.Count > 0)
            {
                Console.WriteLine($"     Indexes:      {table.IndexDefinitions.Count}");
                foreach (var index in table.IndexDefinitions)
                {
                    Console.WriteLine($"       {index}");
                }
                Console.WriteLine();
            }
        }
    }
}
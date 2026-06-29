
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Cli.Services;

/// <summary>
/// Shared helper to print detailed schema information including tables, columns,
/// data types, constraints, and indexes. Used by the debug command and by
/// generate/snapshot commands when --verbose is enabled.
/// </summary>
public static class SchemaDebugPrinter
{
    /// <summary>
    /// Prints a formatted schema summary to the console, showing all tables with
    /// their columns (name, type, attributes), constraints, and indexes.
    /// </summary>
    /// <param name="label">Section header label (e.g., "📊 Schema Summary").</param>
    /// <param name="snapshot">The schema snapshot containing table definitions to print.</param>
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
                    if (constraint.ConstraintType == "FOREIGN KEY")
                    {
                        Console.WriteLine($"       {constraint.ConstraintName} {constraint.ConstraintType} {table.TableName}({string.Join(", ", constraint.Columns)}) -> {constraint.ForeignTable}({string.Join(", ", constraint.ForeignColumns)})");
                    }
                    else if (constraint.ConstraintType == "CHECK")
                    {
                        Console.WriteLine($"       {constraint.ConstraintName} {constraint.ConstraintType} ({constraint.Expression})");
                    }
                    else
                    {
                        Console.WriteLine($"       {constraint.ConstraintName} {constraint.ConstraintType} ({string.Join(", ", constraint.Columns)})");
                    }
                }
                Console.WriteLine();
            }

            // Print indexes
            if (table.IndexDefinitions.Count > 0)
            {
                Console.WriteLine($"     Indexes:      {table.IndexDefinitions.Count}");
                foreach (var index in table.IndexDefinitions)
                {
                    Console.WriteLine($"       {index.IndexName} {index.SchemaName}.{index.TableName}({string.Join(", ", index.Columns)}) {index.IndexType} {(index.IsUnique ? "[UNIQUE]" : "")}" +
                                      (index.Where != null ? $" WHERE {index.Where}" : ""));
                }
                Console.WriteLine();
            }
        }
    }
}
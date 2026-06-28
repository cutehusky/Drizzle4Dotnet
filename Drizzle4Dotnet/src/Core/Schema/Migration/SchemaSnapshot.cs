using System.Text.Json;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// A serializable snapshot of a database schema, used for migration diffing.
/// </summary>
public class SchemaSnapshot
{
    /// <summary>A descriptive name for this snapshot (e.g., "v1.0.0").</summary>
    public string Name { get; set; } = "";

    /// <summary>Tables in this schema snapshot.</summary>
    public List<SnapshotTable> Tables { get; set; } = new();

    /// <summary>
    /// Serializes the snapshot to a JSON string for storage.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Deserializes a snapshot from a JSON string.
    /// </summary>
    public static SchemaSnapshot Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SchemaSnapshot>(json) 
            ?? throw new InvalidOperationException("Failed to deserialize schema snapshot");
    }

    /// <summary>
    /// Creates a snapshot from a list of <see cref="TableDefinition"/> objects.
    /// </summary>
    public static SchemaSnapshot FromTableDefinitions(string name, IReadOnlyList<TableDefinition> tables)
    {
        var snapshot = new SchemaSnapshot { Name = name };
        foreach (var table in tables)
        {
            var snTable = new SnapshotTable
            {
                SchemaName = table.SchemaName,
                TableName = table.TableName
            };
            foreach (var col in table.Columns)
            {
                snTable.Columns.Add(new SnapshotColumn
                {
                    Name = col.Name,
                    DataType = col.DataType,
                    IsNullable = col.IsNullable,
                    IsPrimaryKey = col.IsPrimaryKey,
                    IsAutoIncrement = col.IsAutoIncrement,
                    DefaultValue = col.DefaultValue,
                    CheckExpression = col.CheckExpression
                });
            }
            snapshot.Tables.Add(snTable);
        }
        return snapshot;
    }

    /// <summary>
    /// Compares this snapshot with another and returns the differences.
    /// </summary>
    public SchemaDiff Compare(SchemaSnapshot other)
    {
        var tableChanges = new List<TableChange>();
        
        var myTables = Tables.ToDictionary(t => (t.SchemaName, t.TableName));
        var otherTables = other.Tables.ToDictionary(t => (t.SchemaName, t.TableName));

        // Find added or modified tables
        foreach (var (key, otherTable) in otherTables)
        {
            if (myTables.TryGetValue(key, out var myTable))
            {
                // Table exists in both - compare columns
                var columnChanges = CompareColumnSets(myTable, otherTable);
                if (columnChanges.Count > 0)
                {
                    tableChanges.Add(new TableChange(
                        TableChangeType.Modified,
                        key.TableName,
                        key.SchemaName,
                        columnChanges,
                        ToTableDef(myTable),
                        ToTableDef(otherTable)
                    ));
                }
            }
            else
            {
                // Table was added
                tableChanges.Add(new TableChange(
                    TableChangeType.Added,
                    key.TableName,
                    key.SchemaName,
                    oldTable: null,
                    newTable: ToTableDef(otherTable)
                ));
            }
        }

        // Find removed tables
        foreach (var (key, myTable) in myTables)
        {
            if (!otherTables.ContainsKey(key))
            {
                tableChanges.Add(new TableChange(
                    TableChangeType.Removed,
                    key.TableName,
                    key.SchemaName,
                    oldTable: ToTableDef(myTable),
                    newTable: null
                ));
            }
        }

        return new SchemaDiff(tableChanges);
    }

    private static List<ColumnChange> CompareColumnSets(SnapshotTable oldTable, SnapshotTable newTable)
    {
        var changes = new List<ColumnChange>();
        var oldCols = oldTable.Columns.ToDictionary(c => c.Name);
        var newCols = newTable.Columns.ToDictionary(c => c.Name);

        // Find added or modified columns
        foreach (var (name, newCol) in newCols)
        {
            if (oldCols.TryGetValue(name, out var oldCol))
            {
                // Check for changes
                if (oldCol.DataType != newCol.DataType)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.TypeChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                else if (oldCol.IsNullable != newCol.IsNullable)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.NullabilityChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                else if (oldCol.DefaultValue != newCol.DefaultValue)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.DefaultChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
            }
            else
            {
                // Column was added
                changes.Add(new ColumnChange(ColumnChangeType.Added, name, newDefinition: ToColDef(newCol)));
            }
        }

        // Find removed columns
        foreach (var (name, oldCol) in oldCols)
        {
            if (!newCols.ContainsKey(name))
            {
                changes.Add(new ColumnChange(ColumnChangeType.Removed, name, oldDefinition: ToColDef(oldCol)));
            }
        }

        return changes;
    }

    private static IColumnDefinition ToColDef(SnapshotColumn col)
    {
        return new ColumnDefinition<PgSqlSqlDialectImpl>(col.Name, col.DataType)
        {
            IsNullable = col.IsNullable,
            IsPrimaryKey = col.IsPrimaryKey,
            IsAutoIncrement = col.IsAutoIncrement,
            DefaultValue = col.DefaultValue,
            CheckExpression = col.CheckExpression
        };
    }

    private static TableDefinition ToTableDef(SnapshotTable table)
    {
        var columns = table.Columns.Select(ToColDef).ToList();
        return new TableDefinition(table.TableName, table.SchemaName, columns);
    }
}

/// <summary>
/// Serializable representation of a table for snapshot storage.
/// </summary>
public class SnapshotTable
{
    public string SchemaName { get; set; } = "public";
    public string TableName { get; set; } = "";
    public List<SnapshotColumn> Columns { get; set; } = new();
}

/// <summary>
/// Serializable representation of a column for snapshot storage.
/// </summary>
public class SnapshotColumn
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public bool IsNullable { get; set; } = true;
    public bool IsPrimaryKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string? DefaultValue { get; set; }
    public string? CheckExpression { get; set; }
}

/// <summary>
/// Represents the differences between two schema snapshots.
/// </summary>
public class SchemaDiff
{
    public IReadOnlyList<TableChange> TableChanges { get; }

    public bool HasChanges => TableChanges.Count > 0;

    public SchemaDiff(IReadOnlyList<TableChange> tableChanges)
    {
        TableChanges = tableChanges;
    }

    /// <summary>
    /// Migrates the schema by generating the appropriate DDL statements.
    /// </summary>
    public MigrationPlan ToMigrationPlan(string migrationName)
    {
        var steps = new List<MigrationStep>();

        foreach (var change in TableChanges)
        {
            switch (change.ChangeType)
            {
                case TableChangeType.Added:
                    steps.Add(new MigrationStep(
                        MigrationStepType.CreateTable,
                        $"Create table {change.SchemaName}.{change.TableName}",
                        new CreateTableQuery(change.NewTable!)
                    ));
                    break;

                case TableChangeType.Removed:
                    steps.Add(new MigrationStep(
                        MigrationStepType.DropTable,
                        $"Drop table {change.SchemaName}.{change.TableName}",
                        new DropTableQuery(change.TableName, change.SchemaName)
                    ));
                    break;

                case TableChangeType.Modified:
                    var alterQuery = new AlterTableQuery(change.TableName, change.SchemaName);
                    foreach (var colChange in change.ColumnChanges)
                    {
                        switch (colChange.ChangeType)
                        {
                            case ColumnChangeType.Added:
                                alterQuery.AddColumn(colChange.NewDefinition!);
                                break;
                            case ColumnChangeType.Removed:
                                alterQuery.DropColumn(colChange.ColumnName);
                                break;
                            case ColumnChangeType.TypeChanged:
                                alterQuery.AlterColumnType(colChange.ColumnName, colChange.NewDefinition!.DataType);
                                break;
                            case ColumnChangeType.NullabilityChanged:
                                if (colChange.NewDefinition!.IsNullable)
                                    alterQuery.DropNotNull(colChange.ColumnName);
                                else
                                    alterQuery.SetNotNull(colChange.ColumnName);
                                break;
                            case ColumnChangeType.DefaultChanged:
                                if (colChange.NewDefinition?.DefaultValue != null)
                                    alterQuery.SetDefault(colChange.ColumnName, colChange.NewDefinition.DefaultValue);
                                else
                                    alterQuery.DropDefault(colChange.ColumnName);
                                break;
                        }
                    }
                    steps.Add(new MigrationStep(
                        MigrationStepType.AlterTable,
                        $"Alter table {change.SchemaName}.{change.TableName}",
                        alterQuery
                    ));
                    break;
            }
        }

        return new MigrationPlan(migrationName, steps);
    }
}

/// <summary>
/// Represents a step in a migration plan.
/// </summary>
public class MigrationStep
{
    public MigrationStepType StepType { get; }
    public string Description { get; }
    public ISql Sql { get; }

    public MigrationStep(MigrationStepType stepType, string description, ISql sql)
    {
        StepType = stepType;
        Description = description;
        Sql = sql;
    }

    /// <summary>
    /// Builds the SQL string using the specified dialect.
    /// </summary>
    public string ToSql<TDialect>() where TDialect : ISqlDialect
    {
        var builder = new SqlBuilder<TDialect>();
        Sql.BuildSql(builder);
        return builder.Build().Item1;
    }
}

public enum MigrationStepType
{
    CreateTable,
    DropTable,
    AlterTable,
    CreateIndex,
    DropIndex
}

/// <summary>
/// A complete migration plan consisting of multiple steps.
/// </summary>
public class MigrationPlan
{
    public string Name { get; }
    public IReadOnlyList<MigrationStep> Steps { get; }

    public MigrationPlan(string name, IReadOnlyList<MigrationStep> steps)
    {
        Name = name;
        Steps = steps;
    }

    /// <summary>
    /// Generates the full migration SQL script.
    /// </summary>
    public string ToSql<TDialect>() where TDialect : ISqlDialect
    {
        var parts = new List<string>();
        parts.Add($"-- Migration: {Name}");
        parts.Add($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        parts.Add("");

        foreach (var step in Steps)
        {
            parts.Add($"-- {step.Description}");
            parts.Add(step.ToSql<TDialect>());
            parts.Add("");
        }

        return string.Join("\n", parts);
    }
}

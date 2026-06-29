using System.Text.Json;
using Drizzle4Dotnet.Core.Schema.Migration.Query;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// A serializable snapshot of a database schema, used for migration diffing.
/// </summary>
public class SchemaSnapshot
{
    /// <summary>A descriptive name for this snapshot (e.g., "v1.0.0").</summary>
    public string Name { get; init; } = "";

    /// <summary>Tables in this schema snapshot.</summary>
    public List<SnapshotTable> Tables { get; init; } = new();

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
    /// Uses case-insensitive property matching for robustness with manually edited snapshots.
    /// </summary>
    public static SchemaSnapshot Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SchemaSnapshot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize schema snapshot");
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
                TableName = table.TableName,
                IndexDefinitions = table.Indexes.Select(ToSnapshotIndex).ToList(),
                ConstraintDefinitions = table.TableConstraints.Select(ToSnapshotConstraint).ToList()
            };
            foreach (var col in table.Columns)
            {
                snTable.Columns.Add(new SnapshotColumn
                {
                    Name = col.Name,
                    RawDataType = col.RawDataType,
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
    /// Creates a diff where all tables are marked as Added (used for first migration).
    /// </summary>
    public SchemaDiff CreateNew()
    {
        var tableChanges = Tables.Select(table =>
        {
            var key = (table.SchemaName, table.TableName);
            return new TableChange(
                TableChangeType.Added,
                key.TableName,
                key.SchemaName,
                oldTable: null,
                newTable: ToTableDef(table)
            );
        }).ToList();
        return new SchemaDiff(tableChanges);
    }

    /// <summary>
    /// Creates a diff where all tables are marked as Removed (used for rollback/down of initial migration).
    /// This is the inverse of <see cref="CreateNew"/>.
    /// </summary>
    public SchemaDiff CreateRollback()
    {
        var tableChanges = Tables.Select(table =>
        {
            var key = (table.SchemaName, table.TableName);
            return new TableChange(
                TableChangeType.Removed,
                key.TableName,
                key.SchemaName,
                oldTable: ToTableDef(table),
                newTable: null
            );
        }).ToList();
        return new SchemaDiff(tableChanges);
    }
    
    /// <summary>
    /// Compares this snapshot (the OLD/current state) with <paramref name="newerSnapshot"/> (the NEW/target state)
    /// and returns the differences needed to migrate from this to the new state.
    /// </summary>
    /// <param name="newerSnapshot">The newer/target snapshot to compare against.</param>
    public SchemaDiff Compare(SchemaSnapshot newerSnapshot)
    {
        var currentTables = Tables.ToDictionary(t => (t.SchemaName, t.TableName));
        var newTables = newerSnapshot.Tables.ToDictionary(t => (t.SchemaName, t.TableName));
        var tableChanges = new List<TableChange>();
        
        // Find added or modified tables
        foreach (var (key, newTable) in newTables)
        {
            if (currentTables.TryGetValue(key, out var myTable))
            {
                // Table exists in both - compare columns, constraints, indexes
                var columnChanges = CompareColumnSets(myTable, newTable);

                // Compare constraints using structured data when available (name-based, robust)
                var addedConstraints = CompareConstraintSets(myTable, newTable, added: true);
                var removedConstraints = CompareConstraintSets(myTable, newTable, added: false);

                // Compare indexes using structured data when available (name-based, robust)
                var addedIndexes = CompareIndexSets(myTable, newTable, added: true);
                var removedIndexes = CompareIndexSets(myTable, newTable, added: false);

                if (columnChanges.Count > 0 || addedConstraints.Count > 0 || removedConstraints.Count > 0
                    || addedIndexes.Count > 0 || removedIndexes.Count > 0)
                {
                    tableChanges.Add(new TableChange(
                        TableChangeType.Modified,
                        key.TableName,
                        key.SchemaName,
                        columnChanges,
                        ToTableDef(myTable),
                        ToTableDef(newTable),
                        addedConstraints: addedConstraints,
                        removedConstraints: removedConstraints,
                        addedIndexes: addedIndexes,
                        removedIndexes: removedIndexes
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
                    newTable: ToTableDef(newTable)
                ));
            }
        }

        // Find removed tables
        foreach (var (key, myTable) in currentTables)
        {
            if (!newTables.ContainsKey(key))
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

    /// <summary>
    /// Compares constraint sets between two snapshot tables using structured data.
    /// Both tables must have <see cref="SnapshotTable.ConstraintDefinitions"/> populated.
    /// Returns typed <see cref="TableConstraint"/> objects.
    /// </summary>
    private static List<TableConstraint> CompareConstraintSets(SnapshotTable oldTable, SnapshotTable newTable, bool added)
    {
        var source = added ? newTable : oldTable;
        var target = added ? oldTable : newTable;

        var targetKeys = target.ConstraintDefinitions
            .Select(GetConstraintKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return source.ConstraintDefinitions
            .Where(c => !targetKeys.Contains(GetConstraintKey(c)))
            .Select(c => ToConstraint(source.TableName, c))
            .ToList();
    }

    /// <summary>
    /// Gets a comparison key for a constraint (name if available, otherwise the raw SQL).
    /// </summary>
    private static string GetConstraintKey(SnapshotConstraint c)
    {
        return c.ConstraintName ?? c.RawSql ?? "";
    }

    /// <summary>
    /// Compares index sets between two snapshot tables using structured data.
    /// Both tables must have <see cref="SnapshotTable.IndexDefinitions"/> populated.
    /// Returns typed <see cref="TableIndex"/> objects with full properties.
    /// </summary>
    private static List<TableIndex> CompareIndexSets(SnapshotTable oldTable, SnapshotTable newTable, bool added)
    {
        var source = added ? newTable : oldTable;
        var target = added ? oldTable : newTable;

        var targetNames = target.IndexDefinitions.Select(i => i.IndexName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return source.IndexDefinitions
            .Where(i => !targetNames.Contains(i.IndexName))
            .Select(i => new TableIndex(
                i.IndexName, i.SchemaName, i.TableName,
                i.Columns.ToArray(), i.IsUnique, i.IndexType, i.Where
            ))
            .ToList();
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
                // Check for changes (use independent ifs, not else-if, to detect ALL changes per column)
                if (oldCol.RawDataType != newCol.RawDataType)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.TypeChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                if (oldCol.IsNullable != newCol.IsNullable)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.NullabilityChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                if (oldCol.DefaultValue != newCol.DefaultValue)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.DefaultChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                if (oldCol.IsPrimaryKey != newCol.IsPrimaryKey)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.PrimaryKeyChanged, name, ToColDef(oldCol), ToColDef(newCol)));
                }
                if (oldCol.IsAutoIncrement != newCol.IsAutoIncrement)
                {
                    changes.Add(new ColumnChange(ColumnChangeType.AutoIncrementChanged, name, ToColDef(oldCol), ToColDef(newCol)));
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
        return new RawColumnDefinition(col.Name, col.RawDataType)
        {
            IsNullable = col.IsNullable,
            IsPrimaryKey = col.IsPrimaryKey,
            IsAutoIncrement = col.IsAutoIncrement,
            DefaultValue = col.DefaultValue,
            CheckExpression = col.CheckExpression
        };
    }

    /// <summary>
    /// Converts a <see cref="TableIndex"/> to a serializable <see cref="SnapshotIndex"/>.
    /// </summary>
    private static SnapshotIndex ToSnapshotIndex(TableIndex i)
    {
        return new SnapshotIndex
        {
            IndexName = i.IndexName,
            SchemaName = i.SchemaName,
            TableName = i.TableName,
            Columns = i.Columns.ToList(),
            IsUnique = i.IsUnique,
            IndexType = i.IndexType,
            Where = i.Where
        };
    }

    /// <summary>
    /// Converts a <see cref="TableConstraint"/> to a serializable <see cref="SnapshotConstraint"/>.
    /// </summary>
    private static SnapshotConstraint ToSnapshotConstraint(TableConstraint tc)
    {
        var sn = new SnapshotConstraint { RawSql = tc.ToString() };
        switch (tc)
        {
            case ForeignKeyConstraint fk:
                sn.ConstraintType = "FOREIGN KEY";
                sn.ConstraintName = fk.ConstraintName;
                sn.Columns = fk.Columns.ToList();
                sn.ForeignTable = fk.ForeignTable;
                sn.ForeignSchema = fk.ForeignSchema;
                sn.ForeignColumns = fk.ForeignColumns.ToList();
                break;
            case UniqueConstraint uc:
                sn.ConstraintType = "UNIQUE";
                sn.Columns = uc.Columns.ToList();
                break;
            case PrimaryKeyTableConstraint pk:
                sn.ConstraintType = "PRIMARY KEY";
                sn.Columns = pk.Columns.ToList();
                break;
            case CheckTableConstraint cc:
                sn.ConstraintType = "CHECK";
                sn.Expression = cc.Expression;
                break;
            default:
                sn.ConstraintType = "RAW";
                break;
        }
        return sn;
    }

    /// <summary>
    /// Reconstructs a <see cref="TableConstraint"/> from a <see cref="SnapshotConstraint"/>.
    /// Falls back to <see cref="RawTableConstraint"/> for types that cannot be reconstructed.
    /// </summary>
    private static TableConstraint ToConstraint(string tableName, SnapshotConstraint sc)
    {
        if (!string.IsNullOrEmpty(sc.RawSql) && sc.ConstraintType == "RAW")
            return new RawTableConstraint(sc.RawSql, sc.ConstraintName ?? $"RAW_{tableName}_{sc.RawSql.GetHashCode():X8}");

        switch (sc.ConstraintType)
        {
            case "FOREIGN KEY":
                return new ForeignKeyConstraint(
                    sc.ConstraintName ?? $"FK_{tableName}_{string.Join("_", sc.Columns)}_{sc.ForeignTable}_{string.Join("_", sc.ForeignColumns)}",
                    sc.Columns.ToArray(),
                    sc.ForeignTable ?? "",
                    sc.ForeignColumns.ToArray(),
                    sc.ForeignSchema ?? "public");
            case "UNIQUE":
                return new UniqueConstraint(sc.Columns.ToArray(), sc.ConstraintName ?? $"UQ_{tableName}_{string.Join("_", sc.Columns)}");
            case "PRIMARY KEY":
                return new PrimaryKeyTableConstraint(sc.Columns.ToArray(), sc.ConstraintName ?? $"PK_{tableName}_{string.Join("_", sc.Columns)}");
            case "CHECK":
                return new CheckTableConstraint(sc.Expression ?? "", sc.ConstraintName ?? $"CHK_{tableName}_{(sc.Expression?.GetHashCode() ?? 0):X8}");
            default:
                return new RawTableConstraint(sc.RawSql ?? "", sc.ConstraintName ?? $"RAW_{tableName}_{(sc.RawSql?.GetHashCode() ?? 0):X8}");
        }
    }

    private static TableDefinition ToTableDef(SnapshotTable table)
    {
        var columns = table.Columns.Select(ToColDef).ToList();
        var constraints = table.ConstraintDefinitions.Select(constraint => ToConstraint(table.TableName, constraint)).ToList();
        var indexes = table.IndexDefinitions.Select(idx => new TableIndex(
            idx.IndexName, idx.SchemaName, idx.TableName,
            idx.Columns.ToArray(), idx.IsUnique, idx.IndexType, idx.Where
        )).ToList();
        return new TableDefinition(table.TableName, table.SchemaName, columns, constraints, indexes);
    }

    /// <summary>
    /// Gets the name of a constraint from the typed <see cref="TableConstraint"/> definition.
    /// All constraint types have a non-null <c>ConstraintName</c> property.
    /// </summary>
    public static string GetConstraintName(TableConstraint constraint)
    {
        return constraint switch
        {
            ForeignKeyConstraint fk => fk.ConstraintName,
            UniqueConstraint uc => uc.ConstraintName,
            PrimaryKeyTableConstraint pk => pk.ConstraintName,
            CheckTableConstraint cc => cc.ConstraintName,
            RawTableConstraint rc => rc.ConstraintName,
            _ => throw new InvalidOperationException($"Unknown constraint type: {constraint.GetType().Name}")
        };
    }
}

/// <summary>
/// Serializable representation of a table for snapshot storage.
/// </summary>
public class SnapshotTable
{
    public string SchemaName { get; init; } = "public";
    public string TableName { get; init; } = "";
    public List<SnapshotColumn> Columns { get; init; } = [];

    /// <summary>
    /// Structured index data for proper reconstruction during migration.
    /// </summary>
    public List<SnapshotIndex> IndexDefinitions { get; init; } = [];

    /// <summary>
    /// Structured constraint data for proper reconstruction during migration.
    /// </summary>
    public List<SnapshotConstraint> ConstraintDefinitions { get; init; } = [];
}

/// <summary>
/// Serializable representation of a database index for snapshot storage.
/// </summary>
public class SnapshotIndex
{
    public string IndexName { get; init; } = "";
    public string SchemaName { get; init; } = "public";
    public string TableName { get; init; } = "";
    public List<string> Columns { get; init; } = [];
    public bool IsUnique { get; init; }
    public string? IndexType { get; init; }
    public string? Where { get; init; }
}

/// <summary>
/// Serializable representation of a table constraint for snapshot storage.
/// Supports FOREIGN KEY, UNIQUE, PRIMARY KEY, CHECK, and a RAW fallback.
/// </summary>
public class SnapshotConstraint
{
    /// <summary>Optional constraint name (e.g., "fk_orders_users").</summary>
    public string? ConstraintName { get; set; }

    /// <summary>Constraint type: "FOREIGN KEY", "UNIQUE", "PRIMARY KEY", "CHECK", or "RAW".</summary>
    public string ConstraintType { get; set; } = "";

    /// <summary>Columns involved (for FK, UNIQUE, PRIMARY KEY).</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>Referenced table (for FOREIGN KEY).</summary>
    public string? ForeignTable { get; set; }

    /// <summary>Optional schema of the referenced table (for FOREIGN KEY).</summary>
    public string? ForeignSchema { get; set; }

    /// <summary>Referenced columns (for FOREIGN KEY).</summary>
    public List<string> ForeignColumns { get; set; } = [];

    /// <summary>CHECK expression (for CHECK constraints).</summary>
    public string? Expression { get; set; }

    /// <summary>
    /// Raw SQL fallback. Used for any constraint type that cannot be represented structurally,
    /// and as a backward-compatibility field for deserialized old-format snapshots.
    /// </summary>
    public string? RawSql { get; init; }
}

/// <summary>
/// A non-generic implementation of <see cref="IColumnDefinition"/> used internally
/// by <see cref="SchemaSnapshot"/> to avoid coupling the snapshot system to any
/// specific database dialect's generic type parameter.
/// </summary>
internal class RawColumnDefinition(string name, string dataType) : IColumnDefinition
{
    public string Name { get; } = name;
    public ISqlDataType SqlDataType { get; } = new RawSqlDataType(dataType);
    public string RawDataType => SqlDataType.Sql;
    public bool IsNullable { get; set; } = true;
    public bool IsPrimaryKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string? DefaultValue { get; set; }
    public string? CheckExpression { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Serializable representation of a column for snapshot storage.
/// </summary>
public class SnapshotColumn
{
    public string Name { get; init; } = "";
    public string RawDataType { get; init; } = "";
    public bool IsNullable { get; init; } = true;
    public bool IsPrimaryKey { get; init; }
    public bool IsAutoIncrement { get; init; }
    public string? DefaultValue { get; init; }
    public string? CheckExpression { get; init; }
}

/// <summary>
/// Represents the differences between two schema snapshots.
/// </summary>
public class SchemaDiff(IReadOnlyList<TableChange> tableChanges)
{
    public IReadOnlyList<TableChange> TableChanges { get; } = tableChanges;

    public bool HasChanges => TableChanges.Count > 0;

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
                    // Add index creation steps for the new table
                    foreach (var index in change.NewTable!.Indexes)
                    {
                        var createIndex = new CreateIndexQuery(index.IndexName, change.TableName, change.SchemaName)
                            .On(index.Columns);
                        if (index.IsUnique) createIndex.Unique();
                        if (index.IndexType != null) createIndex.Using(index.IndexType);
                        if (index.Where != null) createIndex.Where(index.Where);
                        steps.Add(new MigrationStep(
                            MigrationStepType.CreateIndex,
                            $"Create index {index.IndexName} on {change.SchemaName}.{change.TableName}",
                            createIndex
                        ));
                    }
                    // Add constraint creation steps for the new table
                    foreach (var constraintSql in change.NewTable!.TableConstraints)
                    {
                        var constraintName = SchemaSnapshot.GetConstraintName(constraintSql);
                        var addConstraint = new AlterTableQuery(change.TableName, change.SchemaName);
                        addConstraint.AddConstraint(constraintSql.ToString());
                        steps.Add(new MigrationStep(
                            MigrationStepType.AlterTable,
                            $"Add constraint {constraintName} on {change.SchemaName}.{change.TableName}",
                            addConstraint
                        ));
                    }
                    break;

                case TableChangeType.Removed:
                    steps.Add(new MigrationStep(
                        MigrationStepType.DropTable,
                        $"Drop table {change.SchemaName}.{change.TableName}",
                        new DropTableQuery(change.TableName, change.SchemaName)
                    ));
                    break;

                case TableChangeType.Modified:
                    var descriptionParts = new List<string> { $"Alter table {change.SchemaName}.{change.TableName}" };

                    // Column changes -> ALTER TABLE
                    if (change.ColumnChanges.Count > 0)
                    {
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
                                    alterQuery.AlterColumnType(colChange.ColumnName, colChange.NewDefinition!.RawDataType);
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
                            $"Alter table {change.SchemaName}.{change.TableName} (columns)",
                            alterQuery
                        ));
                        descriptionParts.Add($"{change.ColumnChanges.Count} column change(s)");
                    }

                    // Added constraints -> ALTER TABLE ADD (use typed constraint's ToString)
                    foreach (var constraint in change.AddedConstraints)
                    {
                        var constraintName = SchemaSnapshot.GetConstraintName(constraint);
                        var addQuery = new AlterTableQuery(change.TableName, change.SchemaName);
                        addQuery.AddConstraint(constraint.ToString());
                        steps.Add(new MigrationStep(
                            MigrationStepType.AlterTable,
                            $"Add constraint {constraintName} on {change.SchemaName}.{change.TableName}",
                            addQuery
                        ));
                    }
                    if (change.AddedConstraints.Count > 0)
                        descriptionParts.Add($"{change.AddedConstraints.Count} constraint(s) added");

                    // Removed constraints -> ALTER TABLE DROP CONSTRAINT
                    foreach (var constraint in change.RemovedConstraints)
                    {
                        var constraintName = SchemaSnapshot.GetConstraintName(constraint);
                        var dropQuery = new AlterTableQuery(change.TableName, change.SchemaName);
                        dropQuery.DropConstraint(constraintName);
                        steps.Add(new MigrationStep(
                            MigrationStepType.AlterTable,
                            $"Drop constraint {constraintName} on {change.SchemaName}.{change.TableName}",
                            dropQuery
                        ));
                    }
                    if (change.RemovedConstraints.Count > 0)
                        descriptionParts.Add($"{change.RemovedConstraints.Count} constraint(s) removed");

                    // Added indexes -> CREATE INDEX (using typed TableIndex with full properties)
                    foreach (var index in change.AddedIndexes)
                    {
                        var createIndex = new CreateIndexQuery(index.IndexName, change.TableName, change.SchemaName)
                            .On(index.Columns);
                        if (index.IsUnique) createIndex.Unique();
                        if (index.IndexType != null) createIndex.Using(index.IndexType);
                        if (index.Where != null) createIndex.Where(index.Where);
                        steps.Add(new MigrationStep(
                            MigrationStepType.CreateIndex,
                            $"Create index {index.IndexName} on {change.SchemaName}.{change.TableName}",
                            createIndex
                        ));
                    }
                    if (change.AddedIndexes.Count > 0)
                        descriptionParts.Add($"{change.AddedIndexes.Count} index(es) added");
                    
                    // Removed indexes -> DROP INDEX (using typed TableIndex)
                    foreach (var index in change.RemovedIndexes)
                    {
                        if (!string.IsNullOrEmpty(index.IndexName))
                        {
                            steps.Add(new MigrationStep(
                                MigrationStepType.DropIndex,
                                $"Drop index {index.IndexName} on {change.SchemaName}.{change.TableName}",
                                new DropIndexQuery(index.IndexName, change.TableName)
                            ));
                        }
                    }
                    if (change.RemovedIndexes.Count > 0)
                        descriptionParts.Add($"{change.RemovedIndexes.Count} index(es) removed");

                    // Use column changes description as the main step description if no other changes produced steps
                    if (change.ColumnChanges.Count == 0 && change.AddedConstraints.Count == 0
                        && change.RemovedConstraints.Count == 0 && change.AddedIndexes.Count == 0
                        && change.RemovedIndexes.Count == 0)
                    {
                        steps.Add(new MigrationStep(
                            MigrationStepType.AlterTable,
                            string.Join(", ", descriptionParts),
                            new AlterTableQuery(change.TableName, change.SchemaName)
                        ));
                    }
                    break;
            }
        }

        return new MigrationPlan(migrationName, steps);
    }
}

/// <summary>
/// Represents a step in a migration plan.
/// </summary>
public class MigrationStep(MigrationStepType stepType, string description, ISql sql)
{
    public MigrationStepType StepType { get; } = stepType;
    public string Description { get; } = description;
    public ISql Sql { get; } = sql;

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
    CreateSchema,
    CreateDatabase,
    CreateTable,
    DropTable,
    AlterTable,
    CreateIndex,
    DropIndex
}

/// <summary>
/// A complete migration plan consisting of multiple steps.
/// </summary>
public class MigrationPlan(string name, IReadOnlyList<MigrationStep> steps)
{
    public string Name { get; } = name;
    public IReadOnlyList<MigrationStep> Steps { get; } = steps;

    /// <summary>
    /// Generates the full migration SQL script.
    /// </summary>
    public string ToSql<TDialect>() where TDialect : ISqlDialect
    {
        var parts = new List<string>
        {
            $"-- Migration: {Name}",
            $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
            ""
        };

        foreach (var step in Steps)
        {
            parts.Add($"-- {step.Description}");
            parts.Add(step.ToSql<TDialect>());
            parts.Add("");
        }

        return string.Join("\n", parts);
    }
}

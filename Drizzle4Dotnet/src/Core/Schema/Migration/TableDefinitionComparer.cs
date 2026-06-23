using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Compares two <see cref="TableDefinition"/> objects and generates the
/// ALTER TABLE / CREATE TABLE / DROP TABLE SQL statements needed to
/// migrate from the old schema to the new schema.
/// </summary>
public static class TableDefinitionComparer
{
    /// <summary>
    /// Result of comparing two TableDefinitions.
    /// </summary>
    public readonly struct TableDiffResult
    {
        /// <summary>SQL statements to execute for migration, in order.</summary>
        public IReadOnlyList<ISql> Statements { get; }

        /// <summary>Human-readable summary of changes.</summary>
        public IReadOnlyList<string> Changes { get; }

        /// <summary>Whether there are any changes.</summary>
        public bool HasChanges => Statements.Count > 0;

        public TableDiffResult(IReadOnlyList<ISql> statements, IReadOnlyList<string> changes)
        {
            Statements = statements;
            Changes = changes;
        }

        /// <summary>
        /// Generates a complete migration SQL script from this diff result.
        /// </summary>
        public string ToSql<TDialect>() where TDialect : ISqlDialect
        {
            return ToSqlScript<TDialect>(this);
        }

        private static string ToSqlScript<TDialect>(TableDiffResult result) where TDialect : ISqlDialect
        {
            var parts = new List<string>();
            parts.Add($"-- Migration generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            foreach (var change in result.Changes)
                parts.Add($"-- {change}");

            if (!result.HasChanges)
            {
                parts.Add("-- No changes detected.");
                return string.Join("\n", parts);
            }

            parts.Add("");

            foreach (var stmt in result.Statements)
            {
                var builder = new SqlBuilder<TDialect>();
                stmt.BuildSql(builder);
                var sql = builder.Build().Item1;
                parts.Add(sql);
                parts.Add("");
            }

            return string.Join("\n", parts);
        }
    }

    /// <summary>
    /// Compares two TableDefinitions and returns the migration statements.
    /// </summary>
    /// <param name="oldTable">The current/old table definition (can be null for new table).</param>
    /// <param name="newTable">The target/new table definition.</param>
    public static TableDiffResult Compare(TableDefinition? oldTable, TableDefinition newTable)
    {
        if (newTable == null)
            throw new ArgumentNullException(nameof(newTable));

        if (oldTable == null)
        {
            // New table - generate CREATE TABLE
            return new TableDiffResult(
                new[] { new CreateTableQuery(newTable) },
                new[] { $"Create table {newTable.SchemaName}.{newTable.TableName}" }
            );
        }

        var statements = new List<ISql>();
        var changes = new List<string>();

        // Check if table name/schema changed → requires DROP + CREATE
        if (oldTable.TableName != newTable.TableName || oldTable.SchemaName != newTable.SchemaName)
        {
            statements.Add(new CreateTableQuery(newTable));
            changes.Add($"Rename table {oldTable.SchemaName}.{oldTable.TableName} → {newTable.SchemaName}.{newTable.TableName} (CREATE NEW)");

            // We can't ALTER table name, so we generate a new CREATE TABLE
            // The old table would need to be dropped separately
            return new TableDiffResult(statements, changes);
        }

        // Same table - compare columns
        var oldColumns = oldTable.Columns.ToDictionary(c => c.Name);
        var newColumns = newTable.Columns.ToDictionary(c => c.Name);

        var alterBuilder = new AlterTableQuery(newTable.TableName, newTable.SchemaName);
        var hasColumnChanges = false;

        // Find added and modified columns
        foreach (var (name, newCol) in newColumns)
        {
            if (oldColumns.TryGetValue(name, out var oldCol))
            {
                // Column exists - check for modifications
                var colChanges = GetColumnModifications(oldCol, newCol);
                foreach (var (action, description) in colChanges)
                {
                    ApplyAlterAction(alterBuilder, action);
                    changes.Add($"  Column {name}: {description}");
                    hasColumnChanges = true;
                }
            }
            else
            {
                // New column - ADD
                alterBuilder.AddColumn(newCol);
                changes.Add($"  Add column {name} ({newCol.DataType})");
                hasColumnChanges = true;
            }
        }

        // Find removed columns
        foreach (var (name, oldCol) in oldColumns)
        {
            if (!newColumns.ContainsKey(name))
            {
                alterBuilder.DropColumn(name);
                changes.Add($"  Drop column {name}");
                hasColumnChanges = true;
            }
        }

        if (hasColumnChanges)
        {
            statements.Add(alterBuilder);
        }

        return new TableDiffResult(statements, changes);
    }

    /// <summary>
    /// Compares multiple table pairs at once.
    /// </summary>
    /// <param name="oldTables">Dictionary of old tables by (schema, name).</param>
    /// <param name="newTables">Dictionary of new tables by (schema, name).</param>
    public static TableDiffResult CompareSets(
        IReadOnlyDictionary<(string Schema, string Name), TableDefinition> oldTables,
        IReadOnlyDictionary<(string Schema, string Name), TableDefinition> newTables)
    {
        var allStatements = new List<ISql>();
        var allChanges = new List<string>();

        // Find added or modified tables
        foreach (var (key, newTable) in newTables)
        {
            oldTables.TryGetValue(key, out var oldTable);
            var result = Compare(oldTable, newTable);
            allStatements.AddRange(result.Statements);
            if (result.HasChanges)
            {
                allChanges.Add($"Table {key.Schema}.{key.Name}:");
                allChanges.AddRange(result.Changes);
            }
        }

        // Find removed tables
        foreach (var (key, oldTable) in oldTables)
        {
            if (!newTables.ContainsKey(key))
            {
                allStatements.Add(new DropTableQuery(oldTable.TableName, oldTable.SchemaName));
                allChanges.Add($"Drop table {key.Schema}.{key.Name}");
            }
        }

        return new TableDiffResult(allStatements, allChanges);
    }

    /// <summary>
    /// Compares two TableDefinitions and generates the complete migration SQL script.
    /// </summary>
    public static string ToSql<TDialect>(TableDefinition? oldTable, TableDefinition newTable)
        where TDialect : ISqlDialect
    {
        var result = Compare(oldTable, newTable);
        return BuildSqlScript<TDialect>(result);
    }

    /// <summary>
    /// Compares sets of tables and generates the complete migration SQL script.
    /// </summary>
    public static string ToSql<TDialect>(
        IReadOnlyDictionary<(string, string), TableDefinition> oldTables,
        IReadOnlyDictionary<(string, string), TableDefinition> newTables)
        where TDialect : ISqlDialect
    {
        var result = CompareSets(oldTables, newTables);
        return BuildSqlScript<TDialect>(result);
    }

    /// <summary>
    /// Convenience: compares two TableDefinition arrays by key.
    /// </summary>
    public static string ToSql<TDialect>(IReadOnlyList<TableDefinition> oldTables, IReadOnlyList<TableDefinition> newTables)
        where TDialect : ISqlDialect
    {
        var oldDict = oldTables.ToDictionary(t => (t.SchemaName, t.TableName));
        var newDict = newTables.ToDictionary(t => (t.SchemaName, t.TableName));
        return ToSql<TDialect>(oldDict, newDict);
    }

    private static string BuildSqlScript<TDialect>(TableDiffResult result) where TDialect : ISqlDialect
    {
        var parts = new List<string>();
        parts.Add($"-- Migration generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        foreach (var change in result.Changes)
            parts.Add($"-- {change}");

        if (!result.HasChanges)
        {
            parts.Add("-- No changes detected.");
            return string.Join("\n", parts);
        }

        parts.Add("");

        foreach (var stmt in result.Statements)
        {
            var builder = new SqlBuilder<TDialect>();
            stmt.BuildSql(builder);
            var sql = builder.Build().Item1;
            parts.Add(sql);
            parts.Add("");
        }

        return string.Join("\n", parts);
    }

    private static List<(AlterAction Action, string Description)> GetColumnModifications(
        ColumnDefinition oldCol, ColumnDefinition newCol)
    {
        var modifications = new List<(AlterAction, string)>();

        // Check data type change
        if (!string.Equals(oldCol.DataType, newCol.DataType, StringComparison.OrdinalIgnoreCase))
        {
            modifications.Add((
                AlterAction.AlterType(newCol.Name, newCol.DataType),
                $"Type: {oldCol.DataType} → {newCol.DataType}"));
        }

        // Check nullability change
        if (oldCol.IsNullable != newCol.IsNullable)
        {
            if (newCol.IsNullable)
                modifications.Add((AlterAction.DropNotNull(newCol.Name), "Now nullable"));
            else
                modifications.Add((AlterAction.SetNotNull(newCol.Name), "Now NOT NULL"));
        }

        // Check default value change
        if (!string.Equals(oldCol.DefaultValue, newCol.DefaultValue, StringComparison.Ordinal))
        {
            if (newCol.DefaultValue != null)
                modifications.Add((
                    AlterAction.SetDefault(newCol.Name, newCol.DefaultValue),
                    $"Default: {oldCol.DefaultValue ?? "(none)"} → {newCol.DefaultValue}"));
            else
                modifications.Add((
                    AlterAction.DropDefault(newCol.Name),
                    $"Drop default (was: {oldCol.DefaultValue})"));
        }

        // Check auto-increment change
        if (oldCol.IsAutoIncrement != newCol.IsAutoIncrement)
        {
            // Auto-increment changes are complex - note as information
            modifications.Add((
                AlterAction.None,
                $"AutoIncrement: {oldCol.IsAutoIncrement} → {newCol.IsAutoIncrement} (may need manual migration)"));
        }

        // Check primary key change
        if (oldCol.IsPrimaryKey != newCol.IsPrimaryKey)
        {
            modifications.Add((
                AlterAction.None,
                $"PrimaryKey: {oldCol.IsPrimaryKey} → {newCol.IsPrimaryKey} (may need manual migration)"));
        }

        return modifications;
    }

    private static void ApplyAlterAction(AlterTableQuery alterBuilder, AlterAction action)
    {
        switch (action.Type)
        {
            case AlterActionType.AlterType:
                alterBuilder.AlterColumnType(action.ColumnName!, action.NewDataType!);
                break;
            case AlterActionType.SetNotNull:
                alterBuilder.SetNotNull(action.ColumnName!);
                break;
            case AlterActionType.DropNotNull:
                alterBuilder.DropNotNull(action.ColumnName!);
                break;
            case AlterActionType.SetDefault:
                alterBuilder.SetDefault(action.ColumnName!, action.NewDefault!);
                break;
            case AlterActionType.DropDefault:
                alterBuilder.DropDefault(action.ColumnName!);
                break;
            case AlterActionType.None:
                break;
        }
    }

    private readonly struct AlterAction
    {
        public AlterActionType Type { get; }
        public string? ColumnName { get; }
        public string? NewDataType { get; }
        public string? NewDefault { get; }

        private AlterAction(AlterActionType type, string? columnName = null,
            string? newDataType = null, string? newDefault = null)
        {
            Type = type;
            ColumnName = columnName;
            NewDataType = newDataType;
            NewDefault = newDefault;
        }

        public static AlterAction AlterType(string col, string type) =>
            new(AlterActionType.AlterType, col, newDataType: type);

        public static AlterAction SetNotNull(string col) =>
            new(AlterActionType.SetNotNull, col);

        public static AlterAction DropNotNull(string col) =>
            new(AlterActionType.DropNotNull, col);

        public static AlterAction SetDefault(string col, string def) =>
            new(AlterActionType.SetDefault, col, newDefault: def);

        public static AlterAction DropDefault(string col) =>
            new(AlterActionType.DropDefault, col);

        public static readonly AlterAction None = new(AlterActionType.None);
    }

    private enum AlterActionType
    {
        None,
        AlterType,
        SetNotNull,
        DropNotNull,
        SetDefault,
        DropDefault,
    }
}

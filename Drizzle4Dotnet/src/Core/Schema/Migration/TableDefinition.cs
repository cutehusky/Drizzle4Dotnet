namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Non-generic interface for accessing column definition properties without dialect knowledge.
/// Used by <see cref="TableDefinition"/> and schema comparison utilities.
/// </summary>
public interface IColumnDefinition
{ 
    /// <summary>The column name in the database.</summary>
    string Name { get; }
    
    /// <summary>The typed SQL data type.</summary>
    ISqlDataType SqlDataType { get; }
    
    /// <summary>The raw SQL type name string (e.g., "BIGINT", "TEXT").</summary>
    string RawDataType { get; }
    
    /// <summary>Whether the column is nullable.</summary>
    bool IsNullable { get; set; }
    
    /// <summary>Whether the column is a primary key.</summary>
    bool IsPrimaryKey { get; set; }
    
    /// <summary>Whether the column has an auto-increment/identity attribute.</summary>
    bool IsAutoIncrement { get; set; }
    
    /// <summary>A default value expression (e.g., "NOW()", "'default'").</summary>
    string? DefaultValue { get; set; }
    
    /// <summary>A CHECK constraint expression.</summary>
    string? CheckExpression { get; set; }
    
    /// <summary>Column comment/description.</summary>
    string? Comment { get; set; }
}

/// <summary>
/// Represents a column definition for DDL generation (CREATE TABLE, ALTER TABLE, etc.).
/// Stores the SQL data type as an <see cref="ISqlDataType"/> for type-safe dialect-specific types.
/// </summary>
public class ColumnDefinition : IColumnDefinition
{
    /// <summary>The column name in the database.</summary>
    public string Name { get; }

    /// <summary>The typed SQL data type.</summary>
    public ISqlDataType SqlDataType { get; }

    /// <summary>The raw SQL type name string (e.g., "BIGINT", "VARCHAR(255)").</summary>
    public string RawDataType => SqlDataType.Sql;

    /// <summary>Whether the column is nullable.</summary>
    public bool IsNullable { get; set; } = true;

    /// <summary>Whether the column is a primary key.</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>Whether the column has an auto-increment/identity attribute.</summary>
    public bool IsAutoIncrement { get; set; }

    /// <summary>A default value expression (e.g., "NOW()", "'default'").</summary>
    public string? DefaultValue { get; set; }

    /// <summary>A CHECK constraint expression.</summary>
    public string? CheckExpression { get; set; }

    /// <summary>Column comment/description.</summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Creates a column definition with a typed SQL data type.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="sqlDataType">The typed SQL data type (e.g., <see cref="PgSqlDataType.BigInt"/>).</param>
    public ColumnDefinition(string name, ISqlDataType sqlDataType)
    {
        Name = name;
        SqlDataType = sqlDataType;
    }

    /// <summary>
    /// Creates a column definition with a raw SQL data type string.
    /// Wraps the string in a <see cref="RawSqlDataType"/>.
    /// </summary>
    public ColumnDefinition(string name, string dataType)
        : this(name, new RawSqlDataType(dataType))
    {
    }
    
    public ColumnDefinition NotNull()
    {
        IsNullable = false;
        return this;
    }

    public ColumnDefinition Nullable()
    {
        IsNullable = true;
        return this;
    }

    public ColumnDefinition PrimaryKey()
    {
        IsPrimaryKey = true;
        IsNullable = false;
        return this;
    }

    public ColumnDefinition AutoIncrement()
    {
        IsAutoIncrement = true;
        return this;
    }

    public ColumnDefinition WithDefault(string defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }

    public ColumnDefinition WithCheck(string checkExpression)
    {
        CheckExpression = checkExpression;
        return this;
    }

    public ColumnDefinition WithComment(string comment)
    {
        Comment = comment;
        return this;
    }
}

/// <summary>
/// Describes the type of column change detected during schema comparison.
/// </summary>
public enum ColumnChangeType
{
    None,
    Added,
    Removed,
    TypeChanged,
    NullabilityChanged,
    DefaultChanged,
    PrimaryKeyChanged,
    AutoIncrementChanged
}

/// <summary>
/// Represents a detected change in a column between two schema snapshots.
/// </summary>
public readonly struct ColumnChange
{
    public ColumnChangeType ChangeType { get; }
    public string ColumnName { get; }
    public IColumnDefinition? OldDefinition { get; }
    public IColumnDefinition? NewDefinition { get; }

    public ColumnChange(
        ColumnChangeType changeType,
        string columnName,
        IColumnDefinition? oldDefinition = null,
        IColumnDefinition? newDefinition = null)
    {
        ChangeType = changeType;
        ColumnName = columnName;
        OldDefinition = oldDefinition;
        NewDefinition = newDefinition;
    }
}

/// <summary>
/// Describes the type of table-level change detected during schema comparison.
/// </summary>
public enum TableChangeType
{
    None,
    Added,
    Removed,
    Modified
}

/// <summary>
/// Represents a detected change in a table between two schema snapshots.
/// Added/Removed constraints and indexes are stored as typed objects
/// (<see cref="TableConstraint"/> and <see cref="TableIndex"/>) instead of
/// raw SQL strings, allowing <see cref="SchemaDiff.ToMigrationPlan"/>
/// to generate complete DDL statements.
/// </summary>
public readonly struct TableChange
{
    public TableChangeType ChangeType { get; }
    public string TableName { get; }
    public string SchemaName { get; }
    public IReadOnlyList<ColumnChange> ColumnChanges { get; }
    public IReadOnlyList<TableConstraint> AddedConstraints { get; }
    public IReadOnlyList<TableConstraint> RemovedConstraints { get; }
    public IReadOnlyList<TableIndex> AddedIndexes { get; }
    public IReadOnlyList<TableIndex> RemovedIndexes { get; }
    public TableDefinition? OldTable { get; }
    public TableDefinition? NewTable { get; }

    public TableChange(
        TableChangeType changeType,
        string tableName,
        string schemaName,
        IReadOnlyList<ColumnChange> columnChanges = null!,
        TableDefinition? oldTable = null,
        TableDefinition? newTable = null,
        IReadOnlyList<TableConstraint>? addedConstraints = null,
        IReadOnlyList<TableConstraint>? removedConstraints = null,
        IReadOnlyList<TableIndex>? addedIndexes = null,
        IReadOnlyList<TableIndex>? removedIndexes = null)
    {
        ChangeType = changeType;
        TableName = tableName;
        SchemaName = schemaName;
        ColumnChanges = columnChanges ?? Array.Empty<ColumnChange>();
        OldTable = oldTable;
        NewTable = newTable;
        AddedConstraints = addedConstraints ?? Array.Empty<TableConstraint>();
        RemovedConstraints = removedConstraints ?? Array.Empty<TableConstraint>();
        AddedIndexes = addedIndexes ?? Array.Empty<TableIndex>();
        RemovedIndexes = removedIndexes ?? Array.Empty<TableIndex>();
    }
}

/// <summary>
/// Defines a complete table structure for DDL generation.
/// Columns are stored via <see cref="IColumnDefinition"/> to support any dialect.
/// </summary>
public class TableDefinition
{
    public string TableName { get; }
    public string SchemaName { get; }
    public IReadOnlyList<IColumnDefinition> Columns { get; }
    public IReadOnlyList<TableConstraint> TableConstraints { get; }
    public IReadOnlyList<TableIndex> Indexes { get; }

    public TableDefinition(
        string tableName,
        string schemaName,
        IReadOnlyList<IColumnDefinition> columns,
        IReadOnlyList<TableConstraint>? tableConstraints = null,
        IReadOnlyList<TableIndex>? indexes = null)
    {
        TableName = tableName;
        SchemaName = schemaName;
        Columns = columns;
        TableConstraints = tableConstraints ?? Array.Empty<TableConstraint>();
        Indexes = indexes ?? Array.Empty<TableIndex>();
    }
}

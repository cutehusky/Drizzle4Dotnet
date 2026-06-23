using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Represents a column definition for DDL generation (CREATE TABLE, ALTER TABLE, etc.).
/// </summary>
public class ColumnDefinition
{
    /// <summary>The column name in the database.</summary>
    public string Name { get; }
    
    /// <summary>The SQL data type (e.g., "BIGINT", "TEXT", "NUMERIC(18,2)").</summary>
    public string DataType { get; }
    
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

    public ColumnDefinition(string name, string dataType)
    {
        Name = name;
        DataType = dataType;
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
    /// <summary>Column exists in both schemas with no changes.</summary>
    None,
    
    /// <summary>Column was added.</summary>
    Added,
    
    /// <summary>Column was removed.</summary>
    Removed,
    
    /// <summary>Column data type changed.</summary>
    TypeChanged,
    
    /// <summary>Column nullability changed.</summary>
    NullabilityChanged,
    
    /// <summary>Column default value changed.</summary>
    DefaultChanged
}

/// <summary>
/// Represents a detected change in a column between two schema snapshots.
/// </summary>
public readonly struct ColumnChange
{
    public ColumnChangeType ChangeType { get; }
    public string ColumnName { get; }
    public ColumnDefinition? OldDefinition { get; }
    public ColumnDefinition? NewDefinition { get; }

    public ColumnChange(
        ColumnChangeType changeType,
        string columnName,
        ColumnDefinition? oldDefinition = null,
        ColumnDefinition? newDefinition = null)
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
    /// <summary>Table exists in both schemas.</summary>
    None,
    
    /// <summary>Table was added.</summary>
    Added,
    
    /// <summary>Table was removed.</summary>
    Removed,
    
    /// <summary>Table columns changed.</summary>
    Modified
}

/// <summary>
/// Represents a detected change in a table between two schema snapshots.
/// </summary>
public readonly struct TableChange
{
    public TableChangeType ChangeType { get; }
    public string TableName { get; }
    public string SchemaName { get; }
    public IReadOnlyList<ColumnChange> ColumnChanges { get; }
    public TableDefinition? OldTable { get; }
    public TableDefinition? NewTable { get; }

    public TableChange(
        TableChangeType changeType,
        string tableName,
        string schemaName,
        IReadOnlyList<ColumnChange> columnChanges = null!,
        TableDefinition? oldTable = null,
        TableDefinition? newTable = null)
    {
        ChangeType = changeType;
        TableName = tableName;
        SchemaName = schemaName;
        ColumnChanges = columnChanges ?? Array.Empty<ColumnChange>();
        OldTable = oldTable;
        NewTable = newTable;
    }
}

/// <summary>
/// Defines a complete table structure for DDL generation.
/// </summary>
public class TableDefinition
{
    /// <summary>The table name.</summary>
    public string TableName { get; }
    
    /// <summary>The schema name.</summary>
    public string SchemaName { get; }
    
    /// <summary>The columns in the table.</summary>
    public IReadOnlyList<ColumnDefinition> Columns { get; }
    
    /// <summary>Additional table constraints (e.g., "UNIQUE(col1, col2)").</summary>
    public IReadOnlyList<string> TableConstraints { get; }

    public TableDefinition(
        string tableName,
        string schemaName,
        IReadOnlyList<ColumnDefinition> columns,
        IReadOnlyList<string>? tableConstraints = null)
    {
        TableName = tableName;
        SchemaName = schemaName;
        Columns = columns;
        TableConstraints = tableConstraints ?? Array.Empty<string>();
    }
}

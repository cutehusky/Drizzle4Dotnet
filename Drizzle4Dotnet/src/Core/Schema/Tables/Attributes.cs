namespace Drizzle4Dotnet.Core.Schema.Tables;

/// <summary>
/// Marks a class as a database table schema definition.
/// </summary>
/// <param name="name">The table name in the database.</param>
/// <param name="schema">The schema name (default: "public").</param>
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string schema = "") : Attribute
{
    /// <summary>The table name in the database.</summary>
    public string Name { get; } = name;

    /// <summary>The schema name.</summary>
    public string Schema { get; } = schema;

    /// <summary>
    /// Optional dialect type (e.g., typeof(PgSqlSqlDialectImpl) or typeof(MySqlSqlDialectImpl)).
    /// If not specified, defaults to PgSqlSqlDialectImpl for backward compatibility.
    /// </summary>
    public Type? Dialect { get; set; }

    /// <summary>
    /// Table-level constraints as raw SQL strings (e.g., "UNIQUE(Name, Email)", 
    /// "CONSTRAINT fk_dept FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)").
    /// Each string is appended after all column definitions in CREATE TABLE.
    /// </summary>
    public string[]? Constraints { get; set; }
}

/// <summary>
/// Marks a class as a table alias for self-joins.
/// </summary>
/// <param name="table">The source table type.</param>
/// <param name="alias">The alias name.</param>
[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute(Type table, string alias) : Attribute
{
    /// <summary>The source table type.</summary>
    public Type Table { get; } = table;

    /// <summary>The alias name.</summary>
    public string Alias { get; } = alias;

    /// <summary>
    /// Optional dialect type (e.g., typeof(PgSqlSqlDialectImpl) or typeof(MySqlSqlDialectImpl)).
    /// If not specified, defaults to PgSqlSqlDialectImpl for backward compatibility.
    /// </summary>
    public Type? Dialect { get; set; }
}

/// <summary>
/// Marks a class as a virtual table (for subqueries, CTEs, etc.).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class VirtualAttribute : Attribute { }

/// <summary>
/// Specifies a FOREIGN KEY constraint on a table.
/// Can be applied multiple times on the same table class.
/// </summary>
/// <param name="constraintName">Optional constraint name (e.g., "FK_Users_Departments").</param>
/// <param name="columns">Source column names (database column names).</param>
/// <param name="foreignTable">The referenced table CLR type.</param>
/// <param name="foreignColumns">Referenced column names (database column names).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ForeignKeyConstraintAttribute(string? constraintName, string[] columns, Type foreignTable, string[] foreignColumns) : Attribute
{
    public string? ConstraintName { get; } = constraintName;
    public string[] Columns { get; } = columns;
    public Type ForeignTable { get; } = foreignTable;
    public string[] ForeignColumns { get; } = foreignColumns;
}

/// <summary>
/// Specifies a UNIQUE constraint on a table.
/// Can be applied multiple times on the same table class.
/// </summary>
/// <param name="columns">Column names that form the unique constraint (database column names).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class UniqueConstraintAttribute(string[] columns) : Attribute
{
    public string[] Columns { get; } = columns;
}

/// <summary>
/// Specifies a table-level PRIMARY KEY constraint.
/// Can be applied multiple times on the same table class.
/// </summary>
/// <param name="columns">Column names that form the primary key (database column names).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PrimaryKeyTableConstraintAttribute(string[] columns) : Attribute
{
    public string[] Columns { get; } = columns;
}

/// <summary>
/// Specifies a CHECK constraint on a table.
/// Can be applied multiple times on the same table class.
/// </summary>
/// <param name="expression">The CHECK constraint expression (e.g., "value > 0").</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CheckTableConstraintAttribute(string expression) : Attribute
{
    public string Expression { get; } = expression;
}

/// <summary>
/// Specifies a database index on a table.
/// Can be applied multiple times on the same table class.
/// </summary>
/// <param name="indexName">The index name.</param>
/// <param name="columns">Column names to index (database column names).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class IndexAttribute(string indexName, string[] columns) : Attribute
{
    public string IndexName { get; } = indexName;
    public string[] Columns { get; } = columns;

    /// <summary>Whether this is a UNIQUE index.</summary>
    public bool IsUnique { get; set; }

    /// <summary>Index type (e.g., "BTREE", "HASH", "GIN").</summary>
    public string? IndexType { get; set; }

    /// <summary>Partial index WHERE condition.</summary>
    public string? Where { get; set; }
}

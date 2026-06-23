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

namespace Drizzle4Dotnet.Core.Schema.Tables;


[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string schema = "") : Attribute
{
    /// <summary>
    /// Optional dialect type (e.g., typeof(PgSqlSqlDialectImpl) or typeof(MySqlSqlDialectImpl)).
    /// If not specified, defaults to PgSqlSqlDialectImpl for backward compatibility.
    /// </summary>
    public Type? Dialect { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute(Type table, string alias) : Attribute
{
    /// <summary>
    /// Optional dialect type (e.g., typeof(PgSqlSqlDialectImpl) or typeof(MySqlSqlDialectImpl)).
    /// If not specified, defaults to PgSqlSqlDialectImpl for backward compatibility.
    /// </summary>
    public Type? Dialect { get; set; }
}


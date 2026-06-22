using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Static utility class providing factory methods for common SQL expression patterns.
/// </summary>
public static class Sql
{
    /// <summary>
    /// Creates a parameterized value expression.
    /// </summary>
    public static SqlValueNode<T> Value<T>(T value) => new SqlValueNode<T>(value);
    
    /// <summary>
    /// Creates a raw (non-typed) SQL fragment.
    /// </summary>
    public static SqlRawNode<object> Raw(string sql) => new SqlRawNode<object>(sql);
    
    /// <summary>
    /// Creates a typed raw SQL fragment.
    /// </summary>
    public static SqlRawNode<T> Raw<T>(string sql) => new SqlRawNode<T>(sql);
    
    /// <summary>
    /// Creates an unsafe literal SQL fragment (no parameterization).
    /// </summary>
    public static SqlRawNode<object> Literal(string sql) => new SqlRawNode<object>(sql);
    
    /// <summary>
    /// Creates a typed unsafe literal SQL fragment.
    /// </summary>
    public static SqlRawNode<T> Literal<T>(string sql) => new SqlRawNode<T>(sql);
    
    /// <summary>
    /// Creates a NULL literal of the specified type.
    /// </summary>
    public static SqlNullNode<T> Null<T>() => new SqlNullNode<T>();
    
    /// <summary>
    /// Creates a DEFAULT keyword expression.
    /// </summary>
    public static SqlDefaultNode Default() => new SqlDefaultNode();
    
    // Note: PostgreSQL-specific INTERVAL and TimeZone factory methods
    // have been moved to PgSqlStatics in the Drizzle4Dotnet.PgSql namespace.
    // Use PgSqlStatics.Interval() and PgSqlStatics.TimeZone() instead.
}

/// <summary>
/// Represents a SQL NULL literal.
/// </summary>
public readonly struct SqlNullNode<T> : IOperator<T>
{
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("NULL");
    }
}

/// <summary>
/// Represents the SQL DEFAULT keyword.
/// </summary>
public readonly struct SqlDefaultNode : IGenericSql
{
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("DEFAULT");
    }
}

// Note: PostgreSQL-specific IntervalNode has been moved to
// PgSql.Namespace.PgIntervalNode in the Drizzle4Dotnet.PgSql namespace.

// Note: PostgreSQL-specific TimeZoneNode has been moved to
// PgSql.Namespace.PgTimeZoneNode in the Drizzle4Dotnet.PgSql namespace.

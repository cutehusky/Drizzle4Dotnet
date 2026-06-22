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
    
    /// <summary>
    /// Creates a PostgreSQL INTERVAL literal: INTERVAL 'amount unit'
    /// Usage: Sql.Interval(1, "day") → INTERVAL '1 day'
    /// </summary>
    public static IntervalNode Interval(int amount, string unit)
        => new IntervalNode(amount, unit);
    
    /// <summary>
    /// Creates a PostgreSQL INTERVAL literal with decimal amount.
    /// </summary>
    public static IntervalNode Interval(double amount, string unit)
        => new IntervalNode(amount, unit);
    
    /// <summary>
    /// Creates a PostgreSQL timezone name as a SQL string literal: 'UTC', 'Asia/Saigon', etc.
    /// Use with AtTimeZone: AtTimeZone(col, Sql.TimeZone("UTC"))
    /// </summary>
    public static TimeZoneNode TimeZone(string timeZone) => new TimeZoneNode(timeZone);
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

public readonly struct IntervalNode : IOperator<DateTime>
{
    private readonly object _amount;
    private readonly string _unit;

    public IntervalNode(int amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public IntervalNode(double amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INTERVAL '").Append(_amount.ToString()!).Append(' ').Append(_unit).Append('\'');
    }
}

/// <summary>
/// Represents a PostgreSQL timezone name as a SQL string literal: 'UTC', 'Asia/Saigon', etc.
/// Used with AT TIME ZONE which requires a literal timezone name, not a parameterized value.
/// </summary>
public readonly struct TimeZoneNode : IOperator<string>
{
    private readonly string _timeZone;

    public TimeZoneNode(string timeZone)
    {
        _timeZone = timeZone;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('\'').Append(_timeZone).Append('\'');
    }
}

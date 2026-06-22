namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

/// <summary>
/// Wraps a literal value as an ISql for use as function arguments, 
/// without requiring a TDialect constraint.
/// </summary>
public readonly struct SqlValueNode<T> : IOperator<T>
{
    private readonly T _value;

    public SqlValueNode(T value)
    {
        _value = value;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        var paramName = sqlBuilder.AddParameter(_value);
        sqlBuilder.Append(paramName);
    }
}

/// <summary>
/// Wraps a raw SQL fragment as an ISql for use in function calls or expressions.
/// </summary>
public readonly struct SqlRawNode<T> : IOperator<T>
{
    private readonly string _sql;

    public SqlRawNode(string sql)
    {
        _sql = sql;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_sql);
    }
}

/// <summary>
/// Wraps an IGenericSql (non-typed) into ISql for type compatibility.
/// </summary>
public readonly struct GenericSqlWrapper<T> : IOperator<T>
{
    private readonly IGenericSql _inner;

    public GenericSqlWrapper(IGenericSql inner)
    {
        _inner = inner;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _inner.BuildSql(sqlBuilder);
    }
}

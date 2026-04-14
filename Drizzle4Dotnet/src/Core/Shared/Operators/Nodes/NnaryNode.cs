using Drizzle4Dotnet.Core.Query.Select;

namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public readonly struct NnaryNode<T, TReturn>: IOperator<TReturn>
{
    private readonly ISql<T>[] _cols;
    private readonly string _seperator;
    public NnaryNode(ISql<T>[] cols,  string seperator = ", ")
    {
        _cols = cols;
        _seperator = seperator;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('(');
        for (int i = 0; i < _cols.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(_seperator);
            _cols[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}


public readonly struct NnaryAnyNode<T, TReturn, TDialect>: IOperator<TReturn> where TDialect : ISqlDialect
{
    private readonly ISql<T> _c1;
    private readonly SqlValue<T, TDialect>[] _cols;
    private readonly string _op;
    private readonly string _seperator;
    public NnaryAnyNode(ISql<T> c1, SqlValue<T, TDialect>[] cols,  string op, string seperator = ", ")
    {
        _c1 = c1;
        _cols = cols;
        _op = op;
        _seperator = seperator;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _c1.BuildSql(sqlBuilder);
        sqlBuilder.Append(' ').Append(_op).Append(" (");
        for (int i = 0; i < _cols.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(_seperator);
            _cols[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}

public readonly struct SqlValue<T, TDialect> where TDialect : ISqlDialect
{
    private readonly ISql<T>? _sql;
    private readonly T? _value;
    private readonly bool _isSql;

    public SqlValue(ISql<T> sql) { _sql = sql; _value = default; _isSql = true; }
    public SqlValue(T value) { _sql = null; _value = value; _isSql = false; }

    public static implicit operator SqlValue<T, TDialect>(T value) => new(value);
    public static implicit operator SqlValue<T, TDialect>(SelectQuery<T, TDialect>? query) 
    {
        if (query == null) return new SqlValue<T, TDialect>(default(T)!);
        return new SqlValue<T, TDialect>(new SqlConverter<T>(query));
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_isSql && _sql != null) 
        {
            sqlBuilder.Append('(');
            _sql.BuildSql(sqlBuilder);
            sqlBuilder.Append(')');
            return;
        }
        
        var paramName = sqlBuilder.AddParameter(_value);
        sqlBuilder.Append(paramName);
    }
}

internal readonly struct SqlConverter<T> : ISql<T>
{
    private readonly ISql<T> _inner;
    public SqlConverter(ISql<T> inner) => _inner = inner;
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _inner.BuildSql(sqlBuilder);
    }
}

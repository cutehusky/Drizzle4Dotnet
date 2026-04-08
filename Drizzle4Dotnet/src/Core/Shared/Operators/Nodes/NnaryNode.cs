using System.Text;
using Drizzle4Dotnet.Core.Query.Select;

namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public class NnaryNode<T, TReturn>: IOperator<TReturn>
{
    private readonly ISql<T>[] _cols;
    private readonly string _seperator;
    public NnaryNode(ISql<T>[] cols,  string seperator = ", ")
    {
        _cols = cols;
        _seperator = seperator;
    }
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var sb = new StringBuilder();
        sb.Append('(');
        for (int i = 0; i < _cols.Length; i++)
        {
            if (i > 0) sb.Append(_seperator);
            sb.Append(_cols[i].BuildSql(parameters));
        }
        sb.Append(')');
        return sb.ToString();
    }
}


public class NnaryAnyNode<T, TReturn, TDialect>: IOperator<TReturn> where TDialect : ISqlDialect
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
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var sb = new StringBuilder();
        sb.Append(_c1.BuildSql(parameters));
        sb.Append(_op);
        sb.Append('(');
        for (int i = 0; i < _cols.Length; i++)
        {
            if (i > 0) sb.Append(_seperator);
            sb.Append(_cols[i].BuildSql(parameters));
        }
        sb.Append(')');
        return sb.ToString();
    }
}

public struct SqlValue<T, TDialect> where TDialect : ISqlDialect
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
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        if (_isSql && _sql != null) return $"({_sql.BuildSql(parameters)})";
        
        var paramName = $"p{parameters.Count}";
        parameters[paramName] = _value;
        return $"@{paramName}";
    }
}

internal class SqlConverter<T> : ISql<T>
{
    private readonly ISql<T> _inner;
    public SqlConverter(ISql<T> inner) => _inner = inner;
    public string BuildSql(Dictionary<string, object?> p) => _inner.BuildSql(p);
}

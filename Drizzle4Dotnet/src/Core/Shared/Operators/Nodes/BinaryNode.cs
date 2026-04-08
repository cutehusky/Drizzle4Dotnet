namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;


public readonly struct BinarySqlValueNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col;
    private readonly T _value;
    private readonly string _operator;

    public BinarySqlValueNode(ISql<T> col, T value,  string @operator)
    {
        _col = col;
        _value = value;
        _operator = @operator;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        string paramName = $"@p{parameters.Count}";
        parameters.Add(paramName, _value);

        return $"{_col.BuildSql(parameters)} {_operator} {paramName}";
    }
}

public readonly struct BinarySqlListValueNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col;
    private readonly IEnumerable<T> _value;
    private readonly string _operator;

    public BinarySqlListValueNode(ISql<T> col, IEnumerable<T> value,  string @operator)
    {
        _col = col;
        _value = value;
        _operator = @operator;
    }
    
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var paramNames = new List<string>();

        foreach (var val in _value)
        {
            string name = $"@p{parameters.Count}";
            parameters.Add(name, val);
            paramNames.Add(name);
        }

        if (paramNames.Count == 0)
        {
            return _operator.Contains("NOT") ? "1=1" : "1=0";
        }

        return $"{_col.BuildSql(parameters)} {_operator} ({string.Join(", ", paramNames)})";
    }
}

public readonly struct BinaryNode<T> : IOperator<T>
{
    private readonly ISql<T> _c1;
    private readonly ISql<T> _c2;
    private readonly string _operator;
    private readonly bool _wrapInParentheses;

    public BinaryNode(ISql<T> c1, ISql<T> c2, string @operator, bool wrapInParentheses = false)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
        _wrapInParentheses = wrapInParentheses;
    }
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return _wrapInParentheses ? $"({_c1.BuildSql(parameters)}) {_operator} ({_c2.BuildSql(parameters)})" : $"{_c1.BuildSql(parameters)} {_operator} {_c2.BuildSql(parameters)}";
    }
}

public readonly struct BinaryNode<T1, T2, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T1> _c1;
    private readonly ISql<T2> _c2;
    private readonly string _operator;
    private readonly bool _wrapInParentheses;

    public BinaryNode(ISql<T1> c1, ISql<T2> c2, string @operator, bool wrapInParentheses = false)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
        _wrapInParentheses = wrapInParentheses;
    }
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return _wrapInParentheses ? $"({_c1.BuildSql(parameters)}) {_operator} ({_c2.BuildSql(parameters)})" : $"{_c1.BuildSql(parameters)} {_operator} {_c2.BuildSql(parameters)}";
    }
}
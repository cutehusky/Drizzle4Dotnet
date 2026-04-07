namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;


public readonly struct BinaryValueNode<T> : IOperator
{
    private readonly ISql<T> _col;
    private readonly T _value;
    private readonly string _operator;

    public BinaryValueNode(ISql<T> col, T value,  string @operator)
    {
        _col = col;
        _value = value;
        _operator = @operator;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        string paramName = $"@p{parameters.Count}";
        parameters.Add(paramName, _value);

        return $"{_col.Sql} {_operator} {paramName}";
    }
}

public readonly struct BinaryListValueNode<T> : IOperator
{
    private readonly ISql<T> _col;
    private readonly IEnumerable<T> _value;
    private readonly string _operator;

    public BinaryListValueNode(ISql<T> col, IEnumerable<T> value,  string @operator)
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

        return $"{_col.Sql} {_operator} ({string.Join(", ", paramNames)})";
    }
}

public readonly struct BinarySqlNode<T1, T2> : IOperator
{
    private readonly ISql<T1> _c1;
    private readonly ISql<T2> _c2;
    private readonly string _operator;

    public BinarySqlNode(ISql<T1> c1, ISql<T2> c2, string @operator)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
    }
    
    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"{_c1.Sql} {_operator} {_c2.Sql}";
    }
}

public readonly struct BinaryNode: IOperator
{
    private readonly IOperator _c1;
    private readonly IOperator _c2;
    private readonly string _operator;
    
    public BinaryNode(IOperator c1, IOperator c2, string @operator)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"({_c1.BuildSql(parameters)}) {_operator} ({_c2.BuildSql(parameters)})";
    }
}

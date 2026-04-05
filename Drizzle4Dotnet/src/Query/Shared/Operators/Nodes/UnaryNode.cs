using Drizzle4Dotnet.Shared;

namespace Drizzle4Dotnet.Query.Shared.Operators.Nodes;

public readonly struct UnarySqlNode<T> : IOperator
{
    private readonly ISql<T> _expression;
    private readonly string _op;
    private readonly bool _prefix;

    public UnarySqlNode(ISql<T> expression, string op, bool prefix = false)
    {
        _expression = expression;
        _op = op;
        _prefix = prefix;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return _prefix 
            ? $"{_op} ({_expression.Sql})"  
            : $"{_expression.Sql} {_op}";
    }
}


public readonly struct UnaryNode : IOperator
{
    private readonly IOperator _c;
    private readonly string _operator;
    private readonly bool _prefix;

    public UnaryNode(IOperator c, string @operator, bool prefix = false)
    {
        _c = c;
        _operator = @operator;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return _prefix 
            ? $"{_operator} ({_c.BuildSql(parameters)})"  
            : $"{_c.BuildSql(parameters)} {_operator}";
    }
}

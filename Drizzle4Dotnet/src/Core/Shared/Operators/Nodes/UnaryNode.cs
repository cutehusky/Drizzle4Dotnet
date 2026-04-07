namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public readonly struct UnaryNode : IOperator
{
    private readonly ISql _expression;
    private readonly string _op;
    private readonly bool _prefix;

    public UnaryNode(ISql expression, string op, bool prefix = false)
    {
        _expression = expression;
        _op = op;
        _prefix = prefix;
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return _prefix 
            ? $"{_op} ({_expression.BuildSql(parameters)})"  
            : $"{_expression.BuildSql(parameters)} {_op}";
    }
}

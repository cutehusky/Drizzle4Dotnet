using System.Text;

namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public readonly struct UnaryNode<T> : IOperator<T>
{
    private readonly ISql<T> _expression;
    private readonly string _op;
    private readonly bool _prefix;

    public UnaryNode(ISql<T> expression, string op, bool prefix = false)
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

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        if (_prefix)
        {
            sb.Append(_op).Append(' ');
            sb.Append('(');
            _expression.BuildSql(parameters, sb);
            sb.Append(')');
        }
        else
        {
            _expression.BuildSql(parameters, sb);
            sb.Append(' ').Append(_op);
        }
    }
}


public readonly struct UnaryNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _expression;
    private readonly string _op;
    private readonly bool _prefix;

    public UnaryNode(ISql<T> expression, string op, bool prefix = false)
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

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        if (_prefix)
        {
            sb.Append(_op).Append(' ');
            sb.Append('(');
            _expression.BuildSql(parameters, sb);
            sb.Append(')');
        }
        else
        {
            _expression.BuildSql(parameters, sb);
            sb.Append(' ').Append(_op);
        }
    }
}
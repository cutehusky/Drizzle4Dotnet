namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public readonly struct UnaryNode<T> : IOperator<T>
{
    private readonly ISql<T> _expression;
    private readonly string _op;
    private readonly bool _prefix;

    /// <summary>
    /// The function/operator name (e.g., "COUNT", "UPPER", "ABS").
    /// </summary>
    public string FunctionName => _op;

    /// <summary>
    /// The argument expression passed to this unary operator.
    /// </summary>
    public ISql<T> Argument => _expression;

    public UnaryNode(ISql<T> expression, string op, bool prefix = false)
    {
        _expression = expression;
        _op = op;
        _prefix = prefix;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_prefix) 
        {
            sqlBuilder.Append(_op).Append(' ');
            sqlBuilder.Append('(');
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append(')');
        }
        else
        {
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append(' ').Append(_op);
        }
    }
}


public readonly struct UnaryNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _expression;
    private readonly string _op;
    private readonly bool _prefix;

    /// <summary>
    /// The function/operator name (e.g., "COUNT", "SUM", "AVG").
    /// </summary>
    public string FunctionName => _op;

    /// <summary>
    /// The argument expression passed to this unary operator.
    /// </summary>
    public ISql<T> Argument => _expression;

    public UnaryNode(ISql<T> expression, string op, bool prefix = false)
    {
        _expression = expression;
        _op = op;
        _prefix = prefix;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_prefix)
        {
            sqlBuilder.Append(_op).Append(' ');
            sqlBuilder.Append('(');
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append(')');
        }
        else
        {
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append(' ').Append(_op);
        }
    }
}
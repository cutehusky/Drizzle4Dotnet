namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

/// <summary>
/// Represents a function call: FUNC_NAME(arg1, arg2, ...)
/// For unary functions like UPPER(col), use UnaryNode with prefix=true.
/// For functions with multiple arguments, use this node.
/// </summary>
public readonly struct FunctionCallNode<TReturn> : IOperator<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql[] _arguments;

    /// <summary>
    /// The function/operator name (e.g., "ROW_NUMBER", "LEAD", "CONCAT_WS").
    /// </summary>
    public string FunctionName => _functionName;

    /// <summary>
    /// The argument expressions passed to this function call.
    /// </summary>
    public IGenericSql[] Arguments => _arguments;

    public FunctionCallNode(string functionName, params IGenericSql[] arguments)
    {
        _functionName = functionName;
        _arguments = (IGenericSql[])arguments.Clone();
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        for (int i = 0; i < _arguments.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            _arguments[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Extension methods for creating function calls with mixed ISql and value arguments.
/// </summary>
public static class FunctionCallExtensions
{
    public static FunctionCallNode<TReturn> ToFunction<TReturn>(this IGenericSql sql, string functionName)
        => new FunctionCallNode<TReturn>(functionName, sql);
}

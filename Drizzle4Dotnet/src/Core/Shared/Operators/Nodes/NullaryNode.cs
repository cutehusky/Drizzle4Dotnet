namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

/// <summary>
/// A function node that takes no arguments, e.g. NOW(), RANDOM().
/// </summary>
public readonly struct NullaryNode<TReturn> : IOperator<TReturn>
{
    private readonly string _functionName;

    public NullaryNode(string functionName)
    {
        _functionName = functionName;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append("()");
    }
}

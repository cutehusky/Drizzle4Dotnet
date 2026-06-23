using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Represents a window function: function_name(args) OVER (...)
/// Combines an aggregate/window function with an OVER clause.
/// </summary>
public readonly struct PgWindowFunctionNode<TReturn> : IOperator<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql[] _arguments;
    private readonly PgOverNode _over;

    public PgWindowFunctionNode(string functionName, PgOverNode over, params IGenericSql[] arguments)
    {
        _functionName = functionName;
        _over = over;
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
        sqlBuilder.Append(") ");
        _over.BuildSql(sqlBuilder);
    }
}

/// <summary>
/// Extension methods to add .Over() to operators and simple column references,
/// enabling fluent syntax: Count(UsersTable.Id).Over(...)
/// </summary>
public static class PgWindowFunctionExtensions
{
    /// <summary>
    /// Appends an OVER clause to any UnaryNode<T> (aggregate function).
    /// </summary>
    public static PgWindowFunctionNode<T> Over<T>(this UnaryNode<T> aggregate, PgOverNode over)
        => new PgWindowFunctionNode<T>(aggregate.Op, over, aggregate.Argument);

    /// <summary>
    /// Appends an OVER clause to any UnaryNode<T, TReturn>.
    /// </summary>
    public static PgWindowFunctionNode<TReturn> Over<T, TReturn>(this UnaryNode<T, TReturn> aggregate, PgOverNode over)
        => new PgWindowFunctionNode<TReturn>(aggregate.Op, over, aggregate.Argument);

    /// <summary>
    /// Appends an OVER clause to any FunctionCallNode<T>.
    /// Supports functions like PgFunctions.RowNumber(), Rank(), Lead(), etc.
    /// </summary>
    public static PgWindowFunctionNode<T> Over<T>(this FunctionCallNode<T> aggregate, PgOverNode over)
        => new PgWindowFunctionNode<T>(aggregate.FunctionName, over, aggregate.Arguments);
}

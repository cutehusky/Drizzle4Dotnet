using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Operators.Nodes;

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
    /// Appends an OVER clause to any FunctionCallNode<T>.
    /// Supports functions like PgFunctions.RowNumber(), Rank(), Lead(), etc.
    /// </summary>
    public static PgWindowFunctionNode<T> Over<T>(this IFunctionCallNode<T> aggregate, PgOverNode over)
        => new(aggregate.FunctionName, over, aggregate.Arguments);
}

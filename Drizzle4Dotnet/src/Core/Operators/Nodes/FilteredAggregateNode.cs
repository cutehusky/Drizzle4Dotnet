using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators.Nodes;

/// <summary>
/// Wraps an aggregate function with a FILTER (WHERE ...) clause (PostgreSQL).
/// Example: COUNT(*) FILTER (WHERE is_active = true)
/// </summary>
public readonly struct FilteredAggregateNode<T> : IOperator<T>
{
    private readonly IGenericSql _aggregate;
    private readonly IGenericSql _filter;

    public FilteredAggregateNode(IGenericSql aggregate, IGenericSql filter)
    {
        _aggregate = aggregate;
        _filter = filter;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _aggregate.BuildSql(sqlBuilder);
        sqlBuilder.Append(" FILTER (WHERE ");
        _filter.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Extension method to add FILTER (WHERE ...) to aggregate functions.
/// </summary>
public static class FilteredAggregateExtensions
{
    public static FilteredAggregateNode<T> Filter<T>(this ISql<T> aggregate, IGenericSql where) =>
        new(aggregate, where);
}

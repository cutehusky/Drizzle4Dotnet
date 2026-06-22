namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

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
    public static FilteredAggregateNode<T> Filter<T>(this IOperator<T> aggregate, IGenericSql where)
        => new FilteredAggregateNode<T>(aggregate, where);
    
    public static FilteredAggregateNode<T> Filter<T>(this UnaryNode<T> aggregate, IGenericSql where)
        => new FilteredAggregateNode<T>(aggregate, where);
    
    public static FilteredAggregateNode<TReturn> Filter<T, TReturn>(this UnaryNode<T, TReturn> aggregate, IGenericSql where)
        => new FilteredAggregateNode<TReturn>(aggregate, where);
}

using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;

namespace Drizzle4Dotnet.Core.Query;


/// <summary>
/// Represents a compound query combining two select queries with UNION, INTERSECT, or EXCEPT,
/// with virtual table support.
/// </summary>
public class CompoundQuery<TReturn, TDialect, TVirtualTable> : QueryBase<TDialect>, IReturning<TReturn, TDialect, TVirtualTable>
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    private readonly IReturning<TReturn, TDialect, TVirtualTable> _left;
    private readonly IReturning<TReturn, TDialect, TVirtualTable> _right;
    private readonly string _operation;

    public ISelectedColumns<TReturn, TDialect, TVirtualTable> SelectedColumns { get; }

    public CompoundQuery(
        IReturning<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right,
        string operation,
        IQueryExecutor<TDialect> executor
    ) : base(executor)
    {
        _left = left;
        _right = right;
        _operation = operation;
        SelectedColumns = left.SelectedColumns;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('(');
        _left.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
        sqlBuilder.Append(' ').Append(_operation).Append(' ');
        sqlBuilder.Append('(');
        _right.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }

    public TVirtualTable AsSubQuery(string alias)
    {
        return (TVirtualTable)TVirtualTable.Create(this, alias, SelectedColumns);
    }
}

/// <summary>
/// Extension methods for set operations on SelectQuery.
/// </summary>
public static class CompoundQueryExtensions
{
    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Union<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> UnionAll<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION ALL", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Intersect<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "INTERSECT", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Except<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "EXCEPT", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Union<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> UnionAll<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION ALL", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Intersect<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "INTERSECT", left.Executor);

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Except<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "EXCEPT", left.Executor);

    // ====== AsRecursiveCte — wrap compound query as a recursive CTE table ======

    /// <summary>
    /// Wraps a compound query with virtual table support as a recursive CTE table.
    /// </summary>
    public static RecursiveCteTable<TReturn, TDialect> AsRecursiveCte<TReturn, TDialect, TVirtualTable>(
        this CompoundQuery<TReturn, TDialect, TVirtualTable> compoundQuery,
        string alias)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
    {
        return new RecursiveCteTable<TReturn, TDialect>(
            alias,
            compoundQuery,
            (ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>>)
                compoundQuery.SelectedColumns);
    }
}

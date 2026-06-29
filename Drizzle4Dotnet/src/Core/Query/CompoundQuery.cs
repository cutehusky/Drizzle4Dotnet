using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


/// <summary>
/// Represents a compound query combining two select queries with UNION, INTERSECT, or EXCEPT,
/// with virtual table support.
/// </summary>
public class CompoundQuery<TReturn, TDialect, TVirtualTable> : 
    QueryBase<TDialect>, IReturning<TReturn, TDialect, TVirtualTable>,
    ISupportCte<CompoundQuery<TReturn, TDialect, TVirtualTable> , TDialect>,
    ISupportOffsetLimit<CompoundQuery<TReturn, TDialect, TVirtualTable>>,
    IAwaitableQuery<List<TReturn>>,
    ISupportOrderBy<CompoundQuery<TReturn, TDialect, TVirtualTable>> where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    private readonly IReturning<TReturn, TDialect, TVirtualTable> _left;
    private readonly IReturning<TReturn, TDialect, TVirtualTable> _right;
    private readonly string _operation;
    private readonly List<(IGenericSql,bool)> _orderBy = new();
    private int? _limit;
    private int? _offset;

    public CompoundQuery<TReturn, TDialect, TVirtualTable> With(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = false;
        CteTables.AddRange(cteTables);
        return this;
    }
    
    public CompoundQuery<TReturn, TDialect, TVirtualTable> WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = true;
        CteTables.AddRange(cteTables);
        return this;
    }
    
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
        ValidateQuery();
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);
        sqlBuilder.Append('(');
        _left.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
        sqlBuilder.Append(' ').Append(_operation).Append(' ');
        sqlBuilder.Append('(');
        _right.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
        SqlStatics.BuildSqlOrderBy(sqlBuilder, _orderBy);
        TDialect.BuildLimitOffset(sqlBuilder, _limit, _offset);
    }

    public TVirtualTable AsSubQuery(string alias)
    {
        return (TVirtualTable)TVirtualTable.Create(this, alias, SelectedColumns);
    }

    public CompoundQuery<TReturn, TDialect, TVirtualTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public CompoundQuery<TReturn, TDialect, TVirtualTable> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    public CompoundQuery<TReturn, TDialect, TVirtualTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBy.Add((col, asc));
        return this;
    }
    
    public CompoundQuery<TReturn, TDialect, TVirtualTable> OrderBy(
        params (IGenericSql col, bool asc)[] columns)
    {
        _orderBy.AddRange(columns);
        return this;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return Executor.ExecuteGetListAsync(this).GetAwaiter();
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

    /// <summary>
    /// Combines two SELECT queries with INTERSECT ALL (supported by PostgreSQL).
    /// Returns rows that appear in both result sets, including duplicates.
    /// </summary>
    public static CompoundQuery<TReturn, TDialect, TVirtualTable> IntersectAll<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "INTERSECT ALL", left.Executor);

    /// <summary>
    /// Combines two SELECT queries with EXCEPT ALL (supported by PostgreSQL).
    /// Returns rows from the left query that are not in the right query, including duplicates.
    /// </summary>
    public static CompoundQuery<TReturn, TDialect, TVirtualTable> ExceptAll<TReturn, TDialect, TVirtualTable>(
        this ReturningQuery<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "EXCEPT ALL", left.Executor);

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

    /// <summary>
    /// Combines two SELECT queries with INTERSECT ALL (supported by PostgreSQL).
    /// Returns rows that appear in both result sets, including duplicates.
    /// </summary>
    public static CompoundQuery<TReturn, TDialect, TVirtualTable> IntersectAll<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "INTERSECT ALL", left.Executor);

    /// <summary>
    /// Combines two SELECT queries with EXCEPT ALL (supported by PostgreSQL).
    /// Returns rows from the left query that are not in the right query, including duplicates.
    /// </summary>
    public static CompoundQuery<TReturn, TDialect, TVirtualTable> ExceptAll<TReturn, TDialect, TVirtualTable>(
        this Query<TReturn, TDialect, TVirtualTable> left,
        IReturning<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "EXCEPT ALL", left.Executor);

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

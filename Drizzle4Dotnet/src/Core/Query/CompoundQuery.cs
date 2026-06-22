using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

/// <summary>
/// Represents a compound query combining two select queries with UNION, INTERSECT, or EXCEPT.
/// Implements IReturning to support ExecuteGetListAsync.
/// </summary>
public class CompoundQuery<TReturn, TDialect> : QueryBase<TDialect>, IReturning<TReturn, TDialect>
    where TDialect : ISqlDialect
{
    private readonly Query<TReturn, TDialect> _left;
    private readonly Query<TReturn, TDialect> _right;
    private readonly string _operation;

    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public CompoundQuery(
        Query<TReturn, TDialect> left,
        Query<TReturn, TDialect> right,
        string operation)
        : base(left.DbClient)
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
}

/// <summary>
/// Represents a compound query combining two select queries with UNION, INTERSECT, or EXCEPT,
/// with virtual table support.
/// </summary>
public class CompoundQuery<TReturn, TDialect, TVirtualTable> : QueryBase<TDialect>, IReturning<TReturn, TDialect, TVirtualTable>
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    private readonly Query<TReturn, TDialect, TVirtualTable> _left;
    private readonly Query<TReturn, TDialect, TVirtualTable> _right;
    private readonly string _operation;

    public ISelectedColumns<TReturn, TDialect, TVirtualTable> SelectedColumns { get; }

    public CompoundQuery(
        Query<TReturn, TDialect, TVirtualTable> left,
        Query<TReturn, TDialect, TVirtualTable> right,
        string operation)
        : base(left.DbClient)
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
    public static CompoundQuery<TReturn, TDialect> Union<TReturn, TDialect>(
        this SelectQuery<TReturn, TDialect> left,
        SelectQuery<TReturn, TDialect> right)
        where TDialect : ISqlDialect
        => new(left, right, "UNION");

    public static CompoundQuery<TReturn, TDialect> UnionAll<TReturn, TDialect>(
        this SelectQuery<TReturn, TDialect> left,
        SelectQuery<TReturn, TDialect> right)
        where TDialect : ISqlDialect
        => new(left, right, "UNION ALL");

    public static CompoundQuery<TReturn, TDialect> Intersect<TReturn, TDialect>(
        this SelectQuery<TReturn, TDialect> left,
        SelectQuery<TReturn, TDialect> right)
        where TDialect : ISqlDialect
        => new(left, right, "INTERSECT");

    public static CompoundQuery<TReturn, TDialect> Except<TReturn, TDialect>(
        this SelectQuery<TReturn, TDialect> left,
        SelectQuery<TReturn, TDialect> right)
        where TDialect : ISqlDialect
        => new(left, right, "EXCEPT");

    // ====== TVirtualTable variants ======

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Union<TReturn, TDialect, TVirtualTable>(
        this SelectQuery<TReturn, TDialect, TVirtualTable> left,
        SelectQuery<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION");

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> UnionAll<TReturn, TDialect, TVirtualTable>(
        this SelectQuery<TReturn, TDialect, TVirtualTable> left,
        SelectQuery<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "UNION ALL");

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Intersect<TReturn, TDialect, TVirtualTable>(
        this SelectQuery<TReturn, TDialect, TVirtualTable> left,
        SelectQuery<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "INTERSECT");

    public static CompoundQuery<TReturn, TDialect, TVirtualTable> Except<TReturn, TDialect, TVirtualTable>(
        this SelectQuery<TReturn, TDialect, TVirtualTable> left,
        SelectQuery<TReturn, TDialect, TVirtualTable> right)
        where TDialect : ISqlDialect
        where TVirtualTable : IVirtualTable<TDialect>
        => new(left, right, "EXCEPT");
}

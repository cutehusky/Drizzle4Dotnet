using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Select;

// TODO: handle offset limit of various dialects (e.g., SQL Server uses OFFSET ... FETCH NEXT)
public class SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>: Query<TReturn, TDialect, TVirtualTable>,
    ISupportWhere<TSelf>,
    ISupportOrderBy<TSelf>,
    ISupportOffsetLimit<TSelf>,
    ISupportDistinct<TSelf>,
    ISupportCte<TSelf, TDialect>,
    IJoin<TSelf, TDialect>
    where TSelf : SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    protected IGenericTable<TDialect>? FromTable;
    protected readonly List<(IGenericTable<TDialect>, string, IGenericSql?)> Joins = new();
    protected readonly List<IGenericSql> Wheres = new();
    protected readonly List<(IGenericSql, bool)> OrderBys = new();
    protected int? LimitValue;
    protected int? OffsetValue;
    protected bool IsDistinct;
    protected readonly List<IGenericSql> GroupBys = new();
    protected readonly List<IGenericSql> Havings = new();

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        IQueryExecutor<TDialect> executor
        ): base(selectedColumns, executor)
    {
    }
    
    public TSelf With(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = false;
        CteTables.AddRange(cteTables);
        return (TSelf)this;
    }
    
    public TSelf WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = true;
        CteTables.AddRange(cteTables);
        return (TSelf)this;
    }
    
    /// <summary>
    /// Validates the query state before building SQL.
    /// Override in dialect-specific subclasses to add custom validation.
    /// </summary>
    protected override void ValidateQuery()
    {
        if (Joins.Count > 0 && FromTable == null)
        {
            throw new InvalidOperationException("Cannot use JOIN without a FROM table. Call From() first.");
        }
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);
        
        sqlBuilder.Append("SELECT ");
        BuildSqlDistinct(sqlBuilder);
        SelectedColumns.BuildSql(sqlBuilder);

        // FROM
        if (FromTable != null)
        {
            sqlBuilder.Append(" FROM ");
            FromTable.BuildRefSql(sqlBuilder);
        }

        SqlStatics.BuildSqlJoins(sqlBuilder, Joins);

        // WHERE
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // GROUP BY
        SqlStatics.BuildClause(sqlBuilder, " GROUP BY ", ", ", GroupBys);

        // HAVING
        SqlStatics.BuildClause(sqlBuilder, " HAVING ", " AND ", Havings, wrapInParentheses: true);

        // ORDER BY
        SqlStatics.BuildSqlOrderBy(sqlBuilder, OrderBys);

        // LIMIT & OFFSET
        TDialect.BuildLimitOffset(sqlBuilder, LimitValue, OffsetValue);
        
        BuildSqlLock(sqlBuilder);
    }
    
    /// <summary>
    /// Hook for dialect-specific locking clause. Override in subclasses if needed.
    /// Default implementation does nothing (no lock support in core SQL).
    /// </summary>
    protected virtual void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        // No-op by default — dialect subclasses override this for lock support.
    }

    public TSelf From(IGenericTable<TDialect> table)
    {
        FromTable = table;
        return (TSelf)this;
    }

            
    public TSelf Where(params IGenericSql[] conditions)
    {
        Wheres.AddRange(conditions);
        return (TSelf)this;
    }
    
    public TSelf Where(IGenericSql conditions)
    {
        Wheres.Add(conditions);
        return (TSelf)this;
    }
    
    public TSelf GroupBy(IGenericSql columns)
    {
        GroupBys.Add(columns);
        return (TSelf)this;
    }
    
    public TSelf GroupBy(params IGenericSql[] columns)
    {
        GroupBys.AddRange(columns);
        return (TSelf)this;
    }

    public TSelf Having(IGenericSql condition)
    {
        Havings.Add(condition);
        return (TSelf)this;
    }
    
    public TSelf Having(params IGenericSql[] conditions)
    {
        Havings.AddRange(conditions);
        return (TSelf)this;
    }

    public TSelf OrderBy(IGenericSql col, bool asc = true)
    {
        OrderBys.Add((col, asc));
        return (TSelf)this;
    }
    
    public TSelf OrderBy(params (IGenericSql col, bool asc)[] columns)
    {
        foreach (var c in columns) OrderBys.Add(c);
        return (TSelf)this;
    }

    public TSelf Limit(int limit)
    {
        LimitValue = limit;
        return (TSelf)this;
    }

    public TSelf Offset(int offset)
    {
        OffsetValue = offset;
        return (TSelf)this;
    }

    // ====== JOINS ======
    protected TSelf JoinInternal(
        IGenericTable<TDialect> table,
        IGenericSql? on,
        string type)
    {
        Joins.Add((table, type, on));
        return (TSelf)this;
    }

    // ====== STANDARD JOINS (supported by all major databases) ======

    public TSelf InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public TSelf LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public TSelf RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public TSelf CrossJoin(IGenericTable<TDialect> table)
        => JoinInternal(table, null, "CROSS");

    public virtual TSelf Distinct()
    {
        IsDistinct = true;
        return (TSelf)this;
    }

    /// <summary>
    /// Hook for dialect-specific DISTINCT rendering (e.g., DISTINCT ON for PostgreSQL).
    /// Default implementation appends "DISTINCT " if _distinct is true.
    /// </summary>
    protected virtual void BuildSqlDistinct(ISqlBuilder sqlBuilder)
    {
        if (IsDistinct) sqlBuilder.Append("DISTINCT ");
    }
}

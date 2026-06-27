using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Select;

public class SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>: Query<TReturn, TDialect, TVirtualTable>,
    ISupportWhere<TSelf>,
    ISupportOrderBy<TSelf>,
    ISupportLimit<TSelf>,
    ISupportCte<TSelf, TDialect>,
    IJoin<TSelf, TDialect>
    where TSelf : SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    protected IGenericTable<TDialect>? _from;
    protected readonly List<(IGenericTable<TDialect>, string, IGenericSql?)> _joins = new();
    protected readonly List<IGenericSql> _wheres = new();
    protected readonly List<(IGenericSql, bool)> _orderBys = new();
    protected int? _limit;
    protected int? _offset;
    protected bool _distinct;
    protected readonly List<IGenericSql> _groupBys = new();
    protected readonly List<IGenericSql> _havings = new();
    protected string? _intoTable;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        IQueryExecutor<TDialect> executor
        ): base(selectedColumns, executor)
    {
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        CteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = true;
        foreach (var t in cteTables) CteTables.Add(t);
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlCte(sqlBuilder);
        
        sqlBuilder.Append("SELECT ");
        if (_distinct) sqlBuilder.Append("DISTINCT ");
        SelectedColumns.BuildSql(sqlBuilder);

        // INTO (SELECT ... INTO table_name)
        if (_intoTable != null)
        {
            sqlBuilder.Append(" INTO ");
            sqlBuilder.Append(TDialect.BuildIdentifier(_intoTable));
        }

        // FROM
        if (_from != null)
        {
            sqlBuilder.Append(" FROM ");
            _from.BuildRefSql(sqlBuilder);
        }

        BuildSqlJoins(sqlBuilder);

        // WHERE
        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);

        // GROUP BY
        AppendClause(sqlBuilder, " GROUP BY ", ", ", _groupBys);

        // HAVING
        AppendClause(sqlBuilder, " HAVING ", " AND ", _havings, wrapInParentheses: true);

        // ORDER BY
        BuildSqlOrderBy(sqlBuilder);

        // LIMIT & OFFSET
        if (_limit.HasValue || _offset.HasValue)
            sqlBuilder.Append(TDialect.BuildLimitOffset(_limit, _offset));
        
        BuildSqlLock(sqlBuilder);
    }
    
    protected void BuildSqlJoins(ISqlBuilder sqlBuilder)
    {
        if (_joins.Count > 0)
        {
            foreach (var (table, type, on) in _joins)
            {
                sqlBuilder.Append(' ').Append(type).Append(" JOIN ");
                table.BuildRefSql(sqlBuilder);
                if (on != null)
                {
                    sqlBuilder.Append(" ON (");
                    on.BuildSql(sqlBuilder);
                    sqlBuilder.Append(')');
                }
            }
        }
    }
    
    protected virtual void BuildSqlOrderBy(ISqlBuilder sqlBuilder)
    {
        if (_orderBys.Count > 0)
        {
            sqlBuilder.Append(" ORDER BY ");
            for (int i = 0; i < _orderBys.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                var (expr, isAsc) = _orderBys[i];
                expr.BuildSql(sqlBuilder);
                sqlBuilder.Append(isAsc ? " ASC" : " DESC");
            }
        }
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
        _from = table;
        return (TSelf)this;
    }

            
    public TSelf Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return (TSelf)this;
    }
    
    public TSelf Where(IGenericSql conditions)
    {
        _wheres.Add(conditions);
        return (TSelf)this;
    }
    
    public TSelf GroupBy(IGenericSql columns)
    {
        _groupBys.Add(columns);
        return (TSelf)this;
    }
    
    public TSelf GroupBy(params IGenericSql[] columns)
    {
        _groupBys.AddRange(columns);
        return (TSelf)this;
    }

    public TSelf Having(IGenericSql condition)
    {
        _havings.Add(condition);
        return (TSelf)this;
    }
    
    public TSelf Having(params IGenericSql[] conditions)
    {
        _havings.AddRange(conditions);
        return (TSelf)this;
    }

    public TSelf OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return (TSelf)this;
    }
    
    public TSelf OrderBy(params (IGenericSql col, bool asc)[] columns)
    {
        foreach (var c in columns) _orderBys.Add(c);
        return (TSelf)this;
    }

    public TSelf Limit(int limit)
    {
        _limit = limit;
        return (TSelf)this;
    }

    public TSelf Offset(int offset)
    {
        _offset = offset;
        return (TSelf)this;
    }

    // ====== JOINS ======
    protected TSelf JoinInternal(
        IGenericTable<TDialect> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return (TSelf)this;
    }

    public TSelf InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public TSelf LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public TSelf RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public TSelf FullJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    public TSelf CrossJoin(IGenericTable<TDialect> table)
        => JoinInternal(table, null, "CROSS");

    // ====== SELECT INTO ======
    public TSelf Into(string tableName)
    {
        _intoTable = tableName;
        return (TSelf)this;
    }

    public TSelf Distinct()
    {
        _distinct = true;
        return (TSelf)this;
    }
}

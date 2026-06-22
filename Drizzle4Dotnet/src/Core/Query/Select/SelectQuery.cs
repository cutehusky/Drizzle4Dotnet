using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Select;

public enum ELockType
{
    None,
    ForUpdate,
    ForShare
}

/// <summary>
/// Core SELECT query builder — produces standard SQL only.
/// Dialect-specific features (LATERAL joins, PG lock types, RETURNING) 
/// are available in dialect-specific subclasses (PgSelectQuery, MySqlSelectQuery, etc.).
/// </summary>
public class SelectQuery<TReturn, TDialect>: Query<TReturn, TDialect> where TDialect : ISqlDialect
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
    protected string? _lockClause;
    protected IGenericColumn[]? _lockColumns;
    protected bool _skipLocked;
    protected bool _nowait;
    protected readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();
    protected bool _recursive;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public SelectQuery<TReturn, TDialect> With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        _recursive = true;
        foreach (var t in cteTables) _cteTables.Add(t);
        return this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlPrefix(sqlBuilder);
        
        sqlBuilder.Append("SELECT ");
        if (_distinct) sqlBuilder.Append("DISTINCT ");
        SelectedColumns.BuildSql(sqlBuilder);

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
    
    /// <summary>
    /// Hook for dialect-specific prefix (e.g., WITH clause). Override in subclasses if needed.
    /// </summary>
    protected virtual void BuildSqlPrefix(ISqlBuilder sqlBuilder)
    {
        // WITH / WITH RECURSIVE
        if (_cteTables.Count > 0)
        {
            sqlBuilder.Append("WITH");
            if (_recursive) sqlBuilder.Append(" RECURSIVE");
            sqlBuilder.Append(' ');
            for (int i = 0; i < _cteTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _cteTables[i].BuildSql(sqlBuilder);
            }
            sqlBuilder.Append(' ');
        }
    }
    
    /// <summary>
    /// Hook for dialect-specific JOIN rendering. Override in subclasses if needed.
    /// </summary>
    protected virtual void BuildSqlJoins(ISqlBuilder sqlBuilder)
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
    
    /// <summary>
    /// Hook for dialect-specific ORDER BY rendering. Override in subclasses if needed.
    /// </summary>
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
    /// </summary>
    protected virtual void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_lockClause != null)
        {
            sqlBuilder.Append(' ').Append(_lockClause);
            if (_lockColumns != null && _lockColumns.Length > 0)
            {
                sqlBuilder.Append(" OF ");
                for (int i = 0; i < _lockColumns.Length; i++)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    _lockColumns[i].BuildSql(sqlBuilder);
                }
            }
            if (_nowait) sqlBuilder.Append(" NOWAIT");
            else if (_skipLocked) sqlBuilder.Append(" SKIP LOCKED");
        }
    }

    public SelectQuery<TReturn, TDialect> From(IGenericTable<TDialect> table)
    {
        _from = table;
        return this;
    }

    
    public SelectQuery<TReturn, TDialect> Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Where(IGenericSql conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy(IGenericSql columns)
    {
        _groupBys.Add(columns);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy(params IGenericSql[] columns)
    {
        _groupBys.AddRange(columns);
        return this;
    }

    public SelectQuery<TReturn, TDialect> Having(IGenericSql condition)
    {
        _havings.Add(condition);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Having(params IGenericSql[] conditions)
    {
        _havings.AddRange(conditions);
        return this;
    }

    public SelectQuery<TReturn, TDialect> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public SelectQuery<TReturn, TDialect> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public SelectQuery<TReturn, TDialect> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // ====== JOINS ======
    protected SelectQuery<TReturn, TDialect> JoinInternal(
        IGenericTable<TDialect> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public SelectQuery<TReturn, TDialect> InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public SelectQuery<TReturn, TDialect> LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public SelectQuery<TReturn, TDialect> RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public SelectQuery<TReturn, TDialect> FullJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    public SelectQuery<TReturn, TDialect> CrossJoin(IGenericTable<TDialect> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Distinct()
    {
        _distinct = true;
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> ForUpdate() { _lockClause = "FOR UPDATE"; _lockColumns = null; _skipLocked = false; _nowait = false; return this; }
    public SelectQuery<TReturn, TDialect> ForShare() { _lockClause = "FOR SHARE"; _lockColumns = null; _skipLocked = false; _nowait = false; return this; }
    public SelectQuery<TReturn, TDialect> For(ELockType lockType)
    {
        _lockClause = lockType switch
        {
            ELockType.ForUpdate => "FOR UPDATE",
            ELockType.ForShare => "FOR SHARE",
            _ => null
        };
        _lockColumns = null;
        _skipLocked = false;
        _nowait = false;
        return this;
    }
    
    // Enhanced overloads with skipLocked/nowait/OF columns
    public SelectQuery<TReturn, TDialect> ForUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR UPDATE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return this; }
    public SelectQuery<TReturn, TDialect> ForShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR SHARE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return this; }
    public SelectQuery<TReturn, TDialect> For(ELockType lockType, bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = lockType switch
        {
            ELockType.ForUpdate => "FOR UPDATE",
            ELockType.ForShare => "FOR SHARE",
            _ => null
        };
        _lockColumns = ofColumns;
        _skipLocked = skipLocked;
        _nowait = nowait;
        return this;
    }
}

public class SelectQuery<TReturn, TDialect, TVirtualTable>: Query<TReturn, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
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
    protected string? _lockClause;
    protected readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();
    protected IGenericColumn[]? _lockColumns;
    protected bool _skipLocked;
    protected bool _nowait;
    protected bool _recursive;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public SelectQuery<TReturn, TDialect, TVirtualTable> With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect, TVirtualTable> WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        _recursive = true;
        foreach (var t in cteTables) _cteTables.Add(t);
        return this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlPrefix(sqlBuilder);
        
        sqlBuilder.Append("SELECT ");
        if (_distinct) sqlBuilder.Append("DISTINCT ");
        SelectedColumns.BuildSql(sqlBuilder);

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
    
    protected virtual void BuildSqlPrefix(ISqlBuilder sqlBuilder)
    {
        // WITH / WITH RECURSIVE
        if (_cteTables.Count > 0)
        {
            sqlBuilder.Append("WITH");
            if (_recursive) sqlBuilder.Append(" RECURSIVE");
            sqlBuilder.Append('\n');
            for (int i = 0; i < _cteTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                sqlBuilder.Append('\n');
                _cteTables[i].BuildSql(sqlBuilder);
            }
            sqlBuilder.Append('\n');
        }
    }
    
    protected virtual void BuildSqlJoins(ISqlBuilder sqlBuilder)
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
    
    protected virtual void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_lockClause != null)
        {
            sqlBuilder.Append(' ').Append(_lockClause);
            if (_lockColumns != null && _lockColumns.Length > 0)
            {
                sqlBuilder.Append(" OF ");
                for (int i = 0; i < _lockColumns.Length; i++)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    _lockColumns[i].BuildSql(sqlBuilder);
                }
            }
            if (_nowait) sqlBuilder.Append(" NOWAIT");
            else if (_skipLocked) sqlBuilder.Append(" SKIP LOCKED");
        }
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> From(IGenericTable<TDialect> table)
    {
        _from = table;
        return this;
    }

            
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Where(IGenericSql conditions)
    {
        _wheres.Add(conditions);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> GroupBy(IGenericSql columns)
    {
        _groupBys.Add(columns);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> GroupBy(params IGenericSql[] columns)
    {
        _groupBys.AddRange(columns);
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Having(IGenericSql condition)
    {
        _havings.Add(condition);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Having(params IGenericSql[] conditions)
    {
        _havings.AddRange(conditions);
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // ====== JOINS ======
    protected  SelectQuery<TReturn, TDialect, TVirtualTable> JoinInternal(
        IGenericTable<TDialect> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> FullJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> CrossJoin(IGenericTable<TDialect> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Distinct()
    {
        _distinct = true;
        return this;
    }

    public SelectQuery<TReturn, TDialect, TVirtualTable> ForUpdate() { _lockClause = "FOR UPDATE"; _lockColumns = null; _skipLocked = false; _nowait = false; return this; }
    public SelectQuery<TReturn, TDialect, TVirtualTable> ForShare() { _lockClause = "FOR SHARE"; _lockColumns = null; _skipLocked = false; _nowait = false; return this; }
    public SelectQuery<TReturn, TDialect, TVirtualTable> For(ELockType lockType)
    {
        _lockClause = lockType switch
        {
            ELockType.ForUpdate => "FOR UPDATE",
            ELockType.ForShare => "FOR SHARE",
            _ => null
        };
        _lockColumns = null;
        _skipLocked = false;
        _nowait = false;
        return this;
    }
    
    public SelectQuery<TReturn, TDialect, TVirtualTable> ForUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR UPDATE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return this; }
    public SelectQuery<TReturn, TDialect, TVirtualTable> ForShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR SHARE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return this; }
    public SelectQuery<TReturn, TDialect, TVirtualTable> For(ELockType lockType, bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = lockType switch
        {
            ELockType.ForUpdate => "FOR UPDATE",
            ELockType.ForShare => "FOR SHARE",
            _ => null
        };
        _lockColumns = ofColumns;
        _skipLocked = skipLocked;
        _nowait = nowait;
        return this;
    }
}
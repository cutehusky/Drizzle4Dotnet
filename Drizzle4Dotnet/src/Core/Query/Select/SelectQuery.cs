using System.Runtime.CompilerServices;
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
public class SelectQuery<TReturn, TDialect, TSelf>: Query<TReturn, TDialect>
    where TSelf : SelectQuery<TReturn, TDialect, TSelf>
    where TDialect : ISqlDialect
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
    protected string? _intoTable;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        _recursive = true;
        foreach (var t in cteTables) _cteTables.Add(t);
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlPrefix(sqlBuilder);
        
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
        _wheres.AddRange(conditions);
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
        IGenericSql on,
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
    {
        _joins.Add((table, "CROSS", null));
        return (TSelf)this;
    }

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
    
    public TSelf ForUpdate() { _lockClause = "FOR UPDATE"; _lockColumns = null; _skipLocked = false; _nowait = false; return (TSelf)this; }
    public TSelf ForShare() { _lockClause = "FOR SHARE"; _lockColumns = null; _skipLocked = false; _nowait = false; return (TSelf)this; }
    public TSelf For(ELockType lockType)
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
        return (TSelf)this;
    }
    
    // Enhanced overloads with skipLocked/nowait/OF columns
    public TSelf ForUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR UPDATE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return (TSelf)this; }
    public TSelf ForShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR SHARE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return (TSelf)this; }
    public TSelf For(ELockType lockType, bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
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
        return (TSelf)this;
    }
}

public class SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>: Query<TReturn, TDialect, TVirtualTable>
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
    protected string? _lockClause;
    protected readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();
    protected IGenericColumn[]? _lockColumns;
    protected bool _skipLocked;
    protected bool _nowait;
    protected bool _recursive;
    protected string? _intoTable;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        _recursive = true;
        foreach (var t in cteTables) _cteTables.Add(t);
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlPrefix(sqlBuilder);
        
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
        IGenericSql on,
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
    {
        _joins.Add((table, "CROSS", null));
        return (TSelf)this;
    }

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
    
    public TSelf ForUpdate() { _lockClause = "FOR UPDATE"; _lockColumns = null; _skipLocked = false; _nowait = false; return (TSelf)this; }
    public TSelf ForShare() { _lockClause = "FOR SHARE"; _lockColumns = null; _skipLocked = false; _nowait = false; return (TSelf)this; }
    public TSelf For(ELockType lockType)
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
        return (TSelf)this;
    }
    
    public TSelf ForUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR UPDATE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return (TSelf)this; }
    public TSelf ForShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns) { _lockClause = "FOR SHARE"; _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait; return (TSelf)this; }
    public TSelf For(ELockType lockType, bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
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
        return (TSelf)this;
    }
}

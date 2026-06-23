using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific SELECT query builder.
/// Extends the standard SelectQuery with MySQL-specific behavior.
/// MySQL does not support LATERAL joins, FOR NO KEY UPDATE, or FOR KEY SHARE.
/// MySQL supports FOR UPDATE and FOR SHARE (table-based locking).
/// </summary>
public class MySqlSelectQuery<TReturn> : SelectQuery<TReturn, MySqlSqlDialectImpl, MySqlSelectQuery<TReturn>>
{
    protected string? _lockClause;
    protected bool _nowait;
    protected bool _skipLocked;

    public MySqlSelectQuery(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl> selectedColumns,
        DbClient<MySqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }

    // ====== MySQL Lock Clauses ======
    // MySQL only supports FOR UPDATE and FOR SHARE (no multi-clause support).
    // The OF clause in MySQL requires table names (not column names).

    public MySqlSelectQuery<TReturn> ForUpdate()
    {
        _lockClause = "FOR UPDATE";
        _nowait = false;
        _skipLocked = false;
        return this;
    }

    public MySqlSelectQuery<TReturn> ForUpdate(bool skipLocked, bool nowait)
    {
        _lockClause = "FOR UPDATE";
        _nowait = nowait;
        _skipLocked = skipLocked;
        return this;
    }

    public MySqlSelectQuery<TReturn> ForShare()
    {
        _lockClause = "FOR SHARE";
        _nowait = false;
        _skipLocked = false;
        return this;
    }

    public MySqlSelectQuery<TReturn> ForShare(bool skipLocked, bool nowait)
    {
        _lockClause = "FOR SHARE";
        _nowait = nowait;
        _skipLocked = skipLocked;
        return this;
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_lockClause == null) return;

        sqlBuilder.Append(' ').Append(_lockClause);

        if (_nowait)
            sqlBuilder.Append(" NOWAIT");
        else if (_skipLocked)
            sqlBuilder.Append(" SKIP LOCKED");
    }
}


/// <summary>
/// MySQL-specific SELECT query builder with virtual table support.
/// </summary>
public class MySqlSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, MySqlSqlDialectImpl, TVirtualTable, MySqlSelectQuery<TReturn, TVirtualTable>>
    where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
{
    protected string? _lockClause;
    protected bool _nowait;
    protected bool _skipLocked;

    public MySqlSelectQuery(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns,
        DbClient<MySqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }

    // ====== MySQL Lock Clauses ======

    public MySqlSelectQuery<TReturn, TVirtualTable> ForUpdate()
    {
        _lockClause = "FOR UPDATE";
        _nowait = false;
        _skipLocked = false;
        return this;
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> ForUpdate(bool skipLocked, bool nowait)
    {
        _lockClause = "FOR UPDATE";
        _nowait = nowait;
        _skipLocked = skipLocked;
        return this;
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> ForShare()
    {
        _lockClause = "FOR SHARE";
        _nowait = false;
        _skipLocked = false;
        return this;
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> ForShare(bool skipLocked, bool nowait)
    {
        _lockClause = "FOR SHARE";
        _nowait = nowait;
        _skipLocked = skipLocked;
        return this;
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_lockClause == null) return;

        sqlBuilder.Append(' ').Append(_lockClause);

        if (_nowait)
            sqlBuilder.Append(" NOWAIT");
        else if (_skipLocked)
            sqlBuilder.Append(" SKIP LOCKED");
    }
}

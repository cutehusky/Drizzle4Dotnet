using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific SELECT query builder with virtual table support.
/// </summary>
public class MySqlSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, MySqlSqlDialectImpl, TVirtualTable, MySqlSelectQuery<TReturn, TVirtualTable>>,
    ILateralJoin<MySqlSelectQuery<TReturn, TVirtualTable>, MySqlSqlDialectImpl>,
    INaturalJoin<MySqlSelectQuery<TReturn, TVirtualTable>, MySqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
{
    private string? _lockClause;
    private bool _nowait;
    private bool _skipLocked;

    public MySqlSelectQuery(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<MySqlSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== MySQL LATERAL Joins (MySQL 8.0.14+) ======

    public MySqlSelectQuery<TReturn, TVirtualTable> InnerLateralJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER LATERAL");

    public MySqlSelectQuery<TReturn, TVirtualTable> LeftLateralJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT LATERAL");

    public MySqlSelectQuery<TReturn, TVirtualTable> CrossLateralJoin(IGenericTable<MySqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS LATERAL");

    // ====== NATURAL JOINS (not supported by SQL Server) ======

    public MySqlSelectQuery<TReturn, TVirtualTable> NaturalJoin(IGenericTable<MySqlSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL");

    public MySqlSelectQuery<TReturn, TVirtualTable> NaturalLeftJoin(IGenericTable<MySqlSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL LEFT");

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

    // ====== MySQL-specific validation ======

    protected override void ValidateQuery()
    {
        base.ValidateQuery();

        if (_lockClause != null && _nowait && _skipLocked)
        {
            throw new InvalidOperationException(
                "NOWAIT and SKIP LOCKED cannot be combined. Use one or the other.");
        }
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

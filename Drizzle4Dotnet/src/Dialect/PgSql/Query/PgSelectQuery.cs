using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// Represents a single PostgreSQL lock clause (e.g., FOR UPDATE OF table NOWAIT).
/// Multiple clauses can be combined in a single SELECT statement.
/// </summary>
public readonly struct PgLockSpec
{
    public string LockType { get; }
    public IGenericTable<PgSqlSqlDialectImpl>[]? Tables { get; }
    public bool NoWait { get; }
    public bool SkipLocked { get; }

    public PgLockSpec(
        string lockType,
        IGenericTable<PgSqlSqlDialectImpl>[]? tables = null,
        bool noWait = false,
        bool skipLocked = false)
    {
        LockType = lockType;
        Tables = tables;
        NoWait = noWait;
        SkipLocked = skipLocked;
    }
}

/// <summary>
/// PostgreSQL-specific SELECT query builder.
/// Extends the standard SelectQuery with PostgreSQL-specific features:
/// - LATERAL joins
/// - FOR NO KEY UPDATE / FOR KEY SHARE / FOR UPDATE / FOR SHARE lock types (table-based)
/// - Multiple lock clause support (e.g., FOR UPDATE OF table1 FOR SHARE OF table2)
/// </summary>
public class PgSelectQuery<TReturn> : SelectQuery<TReturn, PgSqlSqlDialectImpl, PgSelectQuery<TReturn>>,
    ILateralJoin<PgSelectQuery<TReturn>, PgSqlSqlDialectImpl>
{
    protected readonly List<PgLockSpec> _lockClauses = new();

    public PgSelectQuery(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns,
        IQueryExecutor<PgSqlSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== PostgreSQL-specific LATERAL Joins ======
    
    public PgSelectQuery<TReturn> InnerLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on) => JoinInternal(table, on, "INNER LATERAL");

    public PgSelectQuery<TReturn> LeftLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on) => JoinInternal(table, on, "LEFT LATERAL");

    public PgSelectQuery<TReturn> CrossLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table) => JoinInternal(table, null, "CROSS LATERAL");

    // ====== PostgreSQL Lock Clauses ======
    // Each method adds a lock clause to the list, supporting multi-clause combinations.
    // Examples:
    //   .ForUpdate()                           → FOR UPDATE
    //   .ForUpdate(table1, table2)             → FOR UPDATE OF "table1", "table2"
    //   .ForUpdate().ForShare()                → FOR UPDATE FOR SHARE
    //   .ForUpdate(table1).ForShare(table2)    → FOR UPDATE OF "table1" FOR SHARE OF "table2"

    /// <summary>FOR UPDATE</summary>
    public PgSelectQuery<TReturn> ForUpdate(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR UPDATE", tables));
        return this;
    }

    /// <summary>FOR UPDATE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn> ForUpdate(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR UPDATE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR NO KEY UPDATE</summary>
    public PgSelectQuery<TReturn> ForNoKeyUpdate(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR NO KEY UPDATE", tables));
        return this;
    }

    /// <summary>FOR NO KEY UPDATE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn> ForNoKeyUpdate(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR NO KEY UPDATE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR SHARE</summary>
    public PgSelectQuery<TReturn> ForShare(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR SHARE", tables));
        return this;
    }

    /// <summary>FOR SHARE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn> ForShare(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR SHARE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR KEY SHARE</summary>
    public PgSelectQuery<TReturn> ForKeyShare(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR KEY SHARE", tables));
        return this;
    }

    /// <summary>FOR KEY SHARE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn> ForKeyShare(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _lockClauses.Add(new PgLockSpec("FOR KEY SHARE", tables, nowait, skipLocked));
        return this;
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_lockClauses.Count == 0) return;

        foreach (var clause in _lockClauses)
        {
            sqlBuilder.Append(' ').Append(clause.LockType);

            if (clause.Tables is { Length: > 0 })
            {
                sqlBuilder.Append(" OF ");
                for (int i = 0; i < clause.Tables.Length; i++)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    clause.Tables[i].BuildRefSql(sqlBuilder);
                }
            }

            if (clause.NoWait)
                sqlBuilder.Append(" NOWAIT");
            else if (clause.SkipLocked)
                sqlBuilder.Append(" SKIP LOCKED");
        }
    }
}


/// <summary>
/// PostgreSQL-specific SELECT query builder with virtual table support.
/// </summary>
public class PgSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, PgSqlSqlDialectImpl, TVirtualTable, PgSelectQuery<TReturn, TVirtualTable>>,
    ILateralJoin<PgSelectQuery<TReturn, TVirtualTable>, PgSqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    protected readonly List<PgLockSpec> LockClauses = new();

    public PgSelectQuery(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<PgSqlSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== PostgreSQL-specific LATERAL Joins ======
    public PgSelectQuery<TReturn, TVirtualTable> InnerLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER LATERAL");

    public PgSelectQuery<TReturn, TVirtualTable> LeftLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT LATERAL");

    public PgSelectQuery<TReturn, TVirtualTable> CrossLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS LATERAL");

    // ====== PostgreSQL Lock Clauses ======

    /// <summary>FOR UPDATE</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForUpdate(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR UPDATE", tables));
        return this;
    }

    /// <summary>FOR UPDATE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForUpdate(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR UPDATE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR NO KEY UPDATE</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForNoKeyUpdate(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR NO KEY UPDATE", tables));
        return this;
    }

    /// <summary>FOR NO KEY UPDATE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForNoKeyUpdate(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR NO KEY UPDATE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR SHARE</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForShare(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR SHARE", tables));
        return this;
    }

    /// <summary>FOR SHARE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForShare(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR SHARE", tables, nowait, skipLocked));
        return this;
    }

    /// <summary>FOR KEY SHARE</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForKeyShare(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR KEY SHARE", tables));
        return this;
    }

    /// <summary>FOR KEY SHARE with NOWAIT / SKIP LOCKED and target tables</summary>
    public PgSelectQuery<TReturn, TVirtualTable> ForKeyShare(bool skipLocked, bool nowait, params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        LockClauses.Add(new PgLockSpec("FOR KEY SHARE", tables, nowait, skipLocked));
        return this;
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (LockClauses.Count == 0) return;

        foreach (var clause in LockClauses)
        {
            sqlBuilder.Append(' ').Append(clause.LockType);

            if (clause.Tables is { Length: > 0 })
            {
                sqlBuilder.Append(" OF ");
                for (int i = 0; i < clause.Tables.Length; i++)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    clause.Tables[i].BuildRefSql(sqlBuilder);
                }
            }

            if (clause.NoWait)
                sqlBuilder.Append(" NOWAIT");
            else if (clause.SkipLocked)
                sqlBuilder.Append(" SKIP LOCKED");
        }
    }
}

using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific SELECT query builder.
/// Extends the standard SelectQuery with PostgreSQL-specific features:
/// - LATERAL joins
/// - FOR NO KEY UPDATE / FOR KEY SHARE lock types
/// </summary>
public class PgSelectQuery<TReturn> : SelectQuery<TReturn, PgSqlSqlDialectImpl>
{
    public PgSelectQuery(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns,
        DbClient<PgSqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }

    // ====== PostgreSQL-specific LATERAL Joins ======
    
    public PgSelectQuery<TReturn> InnerLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
    {
        _joins.Add((table, "INNER LATERAL", on));
        return this;
    }

    public PgSelectQuery<TReturn> LeftLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
    {
        _joins.Add((table, "LEFT LATERAL", on));
        return this;
    }

    public PgSelectQuery<TReturn> CrossLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table)
    {
        _joins.Add((table, "CROSS LATERAL", null));
        return this;
    }

    // ====== PostgreSQL-specific Lock Types ======
    
    public PgSelectQuery<TReturn> ForNoKeyUpdate()
    {
        _lockClause = "FOR NO KEY UPDATE";
        _lockColumns = null; _skipLocked = false; _nowait = false;
        return this;
    }

    public PgSelectQuery<TReturn> ForKeyShare()
    {
        _lockClause = "FOR KEY SHARE";
        _lockColumns = null; _skipLocked = false; _nowait = false;
        return this;
    }

    public PgSelectQuery<TReturn> ForNoKeyUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = "FOR NO KEY UPDATE";
        _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait;
        return this;
    }

    public PgSelectQuery<TReturn> ForKeyShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = "FOR KEY SHARE";
        _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait;
        return this;
    }
}


/// <summary>
/// PostgreSQL-specific SELECT query builder with virtual table support.
/// </summary>
public class PgSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, PgSqlSqlDialectImpl, TVirtualTable>
    where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    public PgSelectQuery(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns,
        DbClient<PgSqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }

    // ====== PostgreSQL-specific LATERAL Joins ======
    
    public PgSelectQuery<TReturn, TVirtualTable> InnerLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
    {
        _joins.Add((table, "INNER LATERAL", on));
        return this;
    }

    public PgSelectQuery<TReturn, TVirtualTable> LeftLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
    {
        _joins.Add((table, "LEFT LATERAL", on));
        return this;
    }

    public PgSelectQuery<TReturn, TVirtualTable> CrossLateralJoin(IGenericTable<PgSqlSqlDialectImpl> table)
    {
        _joins.Add((table, "CROSS LATERAL", null));
        return this;
    }

    // ====== PostgreSQL-specific Lock Types ======
    
    public PgSelectQuery<TReturn, TVirtualTable> ForNoKeyUpdate()
    {
        _lockClause = "FOR NO KEY UPDATE";
        _lockColumns = null; _skipLocked = false; _nowait = false;
        return this;
    }

    public PgSelectQuery<TReturn, TVirtualTable> ForKeyShare()
    {
        _lockClause = "FOR KEY SHARE";
        _lockColumns = null; _skipLocked = false; _nowait = false;
        return this;
    }

    public PgSelectQuery<TReturn, TVirtualTable> ForNoKeyUpdate(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = "FOR NO KEY UPDATE";
        _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait;
        return this;
    }

    public PgSelectQuery<TReturn, TVirtualTable> ForKeyShare(bool skipLocked, bool nowait, params IGenericColumn[] ofColumns)
    {
        _lockClause = "FOR KEY SHARE";
        _lockColumns = ofColumns; _skipLocked = skipLocked; _nowait = nowait;
        return this;
    }
}

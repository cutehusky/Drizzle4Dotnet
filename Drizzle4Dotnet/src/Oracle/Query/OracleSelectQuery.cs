using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Query;

/// <summary>
/// Oracle-specific SELECT query builder.
/// Supports Oracle-specific features:
/// - FROM DUAL fallback (Oracle requires DUAL for scalar SELECTs)
/// - OFFSET/FETCH pagination (Oracle 12c+)
/// - FOR UPDATE with WAIT/NOWAIT/SKIP LOCKED
/// - FULL OUTER JOIN, NATURAL JOIN, LATERAL JOIN
/// </summary>
public class OracleSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, OracleSqlDialectImpl, TVirtualTable, OracleSelectQuery<TReturn, TVirtualTable>>,
    IFullOuterJoin<OracleSelectQuery<TReturn, TVirtualTable>, OracleSqlDialectImpl>,
    INaturalJoin<OracleSelectQuery<TReturn, TVirtualTable>, OracleSqlDialectImpl>
    where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
{
    private int? _forUpdateWaitSeconds;
    private bool _forUpdateNoWait;
    private bool _forUpdateSkipLocked;

    public OracleSelectQuery(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<OracleSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ======================================================================
    // FOR UPDATE — Oracle-specific locking clause
    // ======================================================================

    /// <summary>FOR UPDATE — wait indefinitely for the lock.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> ForUpdate()
    {
        _forUpdateWaitSeconds = null;
        _forUpdateNoWait = false;
        _forUpdateSkipLocked = false;
        return this;
    }

    /// <summary>FOR UPDATE WAIT n — wait up to n seconds for the lock.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> ForUpdateWait(int seconds)
    {
        _forUpdateWaitSeconds = seconds;
        _forUpdateNoWait = false;
        _forUpdateSkipLocked = false;
        return this;
    }

    /// <summary>FOR UPDATE NOWAIT — fail immediately if the row is locked.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> ForUpdateNoWait()
    {
        _forUpdateWaitSeconds = null;
        _forUpdateNoWait = true;
        _forUpdateSkipLocked = false;
        return this;
    }

    /// <summary>FOR UPDATE SKIP LOCKED — skip locked rows.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> ForUpdateSkipLocked()
    {
        _forUpdateWaitSeconds = null;
        _forUpdateNoWait = false;
        _forUpdateSkipLocked = true;
        return this;
    }

    // ======================================================================
    // FULL OUTER JOIN
    // ======================================================================

    /// <summary>FULL OUTER JOIN with ON condition.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> FullJoin(IGenericTable<OracleSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    // ======================================================================
    // NATURAL JOIN
    // ======================================================================

    /// <summary>NATURAL JOIN.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> NaturalJoin(IGenericTable<OracleSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL");

    /// <summary>NATURAL LEFT JOIN.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> NaturalLeftJoin(IGenericTable<OracleSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL LEFT");

    /// <summary>NATURAL RIGHT JOIN.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> NaturalRightJoin(IGenericTable<OracleSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL RIGHT");

    /// <summary>NATURAL FULL OUTER JOIN.</summary>
    public OracleSelectQuery<TReturn, TVirtualTable> NaturalFullJoin(IGenericTable<OracleSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL FULL");

    // ======================================================================
    // BuildSql overrides
    // ======================================================================

    protected override void ValidateQuery()
    {
        base.ValidateQuery();
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        if (_forUpdateNoWait)
        {
            sqlBuilder.Append(" FOR UPDATE NOWAIT");
        }
        else if (_forUpdateWaitSeconds.HasValue)
        {
            sqlBuilder.Append(" FOR UPDATE WAIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(_forUpdateWaitSeconds.Value));
        }
        else if (_forUpdateSkipLocked)
        {
            sqlBuilder.Append(" FOR UPDATE SKIP LOCKED");
        }
        // Default FOR UPDATE (no options) is not added by default
        // Users call ForUpdate() explicitly even without options
    }

    // ======================================================================
    // Custom BuildSql with DUAL and Oracle-specific features
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        // SELECT with optional DISTINCT
        sqlBuilder.Append("SELECT ");
        BuildSqlDistinct(sqlBuilder);
        SelectedColumns.BuildSql(sqlBuilder);

        // FROM — handle DUAL fallback
        if (FromTable != null)
        {
            sqlBuilder.Append(" FROM ");
            FromTable.BuildRefSql(sqlBuilder);
        }
        else if (!HasExplicitFrom())
        {
            // Oracle requires FROM DUAL for any SELECT that doesn't reference a table
            sqlBuilder.Append(" FROM DUAL");
        }

        // JOINs
        SqlStatics.BuildSqlJoins(sqlBuilder, Joins);

        // WHERE
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // GROUP BY
        SqlStatics.BuildClause(sqlBuilder, " GROUP BY ", ", ", GroupBys);

        // HAVING
        SqlStatics.BuildClause(sqlBuilder, " HAVING ", " AND ", Havings, wrapInParentheses: true);

        // ORDER BY
        if (OrderBys.Count > 0)
        {
            SqlStatics.BuildSqlOrderBy(sqlBuilder, OrderBys);
        }

        // OFFSET/FETCH (Oracle 12c+)
        OracleSqlDialectImpl.BuildLimitOffset(sqlBuilder, LimitValue, OffsetValue);

        // FOR UPDATE
        BuildSqlLock(sqlBuilder);
    }

    /// <summary>
    /// Determines whether the query has an explicit FROM clause (table or subquery).
    /// </summary>
    private bool HasExplicitFrom()
    {
        return FromTable != null;
    }
}

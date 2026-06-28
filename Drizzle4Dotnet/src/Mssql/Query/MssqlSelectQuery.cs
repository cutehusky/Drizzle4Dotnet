using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Mssql.Operators.Nodes;

namespace Drizzle4Dotnet.Mssql.Query;

/// <summary>
/// MSSQL-specific SELECT query builder.
/// Supports MSSQL-specific features:
/// - TOP(n) clause
/// - OFFSET/FETCH pagination (requires ORDER BY)
/// - CROSS/OUTER APPLY (MSSQL alternative to LATERAL JOIN)
/// - FULL OUTER JOIN
/// - Table hints WITH (NOLOCK)
/// </summary>
public class MssqlSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, MssqlSqlDialectImpl, TVirtualTable, MssqlSelectQuery<TReturn, TVirtualTable>>,
    IFullOuterJoin<MssqlSelectQuery<TReturn, TVirtualTable>, MssqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
{
    private MssqlTopNode? _topNode;
    private readonly List<MssqlTableHintNode> _tableHints = new();

    public MssqlSelectQuery(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<MssqlSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ======================================================================
    // TOP(n) — MSSQL-specific limit clause
    // ======================================================================

    /// <summary>Adds TOP (n) to the SELECT clause.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> Top(int count)
    {
        _topNode = new MssqlTopNode(count);
        return this;
    }

    /// <summary>Adds TOP (n) WITH TIES to the SELECT clause (requires ORDER BY).</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> TopWithTies(int count)
    {
        _topNode = new MssqlTopNode(count, withTies: true);
        return this;
    }

    /// <summary>Adds TOP (n) PERCENT to the SELECT clause.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> TopPercent(int count)
    {
        _topNode = new MssqlTopNode(count, percent: true);
        return this;
    }

    // ======================================================================
    // CROSS / OUTER APPLY (MSSQL alternative to LATERAL JOIN)
    // ======================================================================

    /// <summary>CROSS APPLY — like CROSS LATERAL JOIN.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> CrossApply(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS APPLY");

    /// <summary>OUTER APPLY — like LEFT LATERAL JOIN.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> OuterApply(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "OUTER APPLY");

    // ======================================================================
    // FULL OUTER JOIN
    // ======================================================================

    /// <summary>FULL OUTER JOIN with ON condition.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> FullJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    // ======================================================================
    // Table Hints
    // ======================================================================

    /// <summary>Adds a table hint: WITH (NOLOCK), WITH (TABLOCK), etc.</summary>
    public MssqlSelectQuery<TReturn, TVirtualTable> WithHint(MssqlTableHintNode hint)
    {
        _tableHints.Add(hint);
        return this;
    }

    // ======================================================================
    // BuildSql overrides
    // ======================================================================

    protected override void ValidateQuery()
    {
        base.ValidateQuery();
    }

    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        // MSSQL does not have FOR UPDATE in the same way as PostgreSQL/MySQL.
        // Table hints (WITH (NOLOCK), etc.) are used instead.
        // Lock hints are typically applied per-table, handled separately.
    }

    // ======================================================================
    // Custom BuildSql with TOP and OFFSET/FETCH after ORDER BY
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        // SELECT with optional TOP
        sqlBuilder.Append("SELECT ");
        if (_topNode != null)
        {
            _topNode.BuildSql(sqlBuilder);
        }
        if (_topNode == null && IsDistinct)
        {
            sqlBuilder.Append("DISTINCT ");
        }
        SelectedColumns.BuildSql(sqlBuilder);

        // FROM
        if (FromTable != null)
        {
            sqlBuilder.Append(" FROM ");
            FromTable.BuildRefSql(sqlBuilder);
            // Table hints
            foreach (var hint in _tableHints)
            {
                hint.BuildSql(sqlBuilder);
            }
        }

        // JOINs (including CROSS/OUTER APPLY)
        SqlStatics.BuildSqlJoins(sqlBuilder, Joins);

        // WHERE
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // GROUP BY
        SqlStatics.BuildClause(sqlBuilder, " GROUP BY ", ", ", GroupBys);

        // HAVING
        SqlStatics.BuildClause(sqlBuilder, " HAVING ", " AND ", Havings, wrapInParentheses: true);

        // ORDER BY + OFFSET/FETCH (MSSQL requires OFFSET/FETCH after ORDER BY)
        if (OrderBys.Count > 0)
        {
            SqlStatics.BuildSqlOrderBy(sqlBuilder, OrderBys);
        }
        else if (LimitValue.HasValue || OffsetValue.HasValue)
        {
            // MSSQL requires ORDER BY for OFFSET/FETCH, so add ORDER BY (SELECT NULL) as workaround
            sqlBuilder.Append(" ORDER BY (SELECT 0)");
        }

        // OFFSET/FETCH
        MssqlSqlDialectImpl.BuildLimitOffset(sqlBuilder, LimitValue, OffsetValue);

        BuildSqlLock(sqlBuilder);
    }
}

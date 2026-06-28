using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Query;

/// <summary>
/// Oracle-specific DELETE query builder.
/// Extends DeleteQuery with:
/// - RETURNING ... INTO clause (Oracle's mechanism for returning values from DML)
/// - Note: Oracle does NOT support DELETE ... USING. Use subqueries in WHERE instead.
/// </summary>
public class OracleDeleteQuery<TTable> : DeleteQuery<TTable, OracleSqlDialectImpl, OracleDeleteQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
{
    private readonly List<string> _returningColumns = new();

    public OracleDeleteQuery(TTable table, IQueryExecutor<OracleSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // RETURNING ... INTO clause
    // ======================================================================

    /// <summary>
    /// Adds RETURNING col1, col2 INTO :outP0, :outP1 to capture deleted values.
    /// Oracle syntax: DELETE FROM table WHERE condition RETURNING col1 INTO :outP0
    /// </summary>
    public OracleDeleteQuery<TTable> ReturningInto(params string[] columnNames)
    {
        _returningColumns.AddRange(columnNames);
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("DELETE FROM ");
        Table.BuildRefSql(sqlBuilder);

        // WHERE
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // RETURNING ... INTO
        BuildReturningClause(sqlBuilder);
    }

    /// <summary>
    /// Builds the RETURNING ... INTO clause.
    /// Oracle syntax: RETURNING col1, col2 INTO :outP0, :outP1
    /// </summary>
    private void BuildReturningClause(ISqlBuilder sqlBuilder)
    {
        if (_returningColumns.Count == 0) return;

        sqlBuilder.Append(" RETURNING ");
        for (int i = 0; i < _returningColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(_returningColumns[i]));
        }

        sqlBuilder.Append(" INTO ");
        for (int i = 0; i < _returningColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append($":outP{i}");
        }
    }
}

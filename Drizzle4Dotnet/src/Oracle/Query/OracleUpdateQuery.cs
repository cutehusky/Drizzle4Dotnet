using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Query;

/// <summary>
/// Oracle-specific UPDATE query builder.
/// Extends UpdateQuery with:
/// - RETURNING ... INTO clause (Oracle's mechanism for returning values from DML)
/// - Note: Oracle does NOT support UPDATE ... FROM or UPDATE ... JOIN.
///   Use correlated subqueries in SET clauses instead.
/// </summary>
public class OracleUpdateQuery<TTable> : UpdateQuery<TTable, OracleSqlDialectImpl, OracleUpdateQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
{
    private readonly List<string> _returningColumns = new();

    public OracleUpdateQuery(TTable table, IQueryExecutor<OracleSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // RETURNING ... INTO clause
    // ======================================================================

    /// <summary>
    /// Adds RETURNING col1, col2 INTO :outP0, :outP1 to capture updated values.
    /// Oracle syntax: UPDATE table SET col = val WHERE condition RETURNING col1 INTO :outP0
    /// </summary>
    public OracleUpdateQuery<TTable> ReturningInto(params string[] columnNames)
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

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);

        // SET clause
        SqlStatics.BuildSqlSetClause<OracleSqlDialectImpl>(sqlBuilder, SetValues);

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

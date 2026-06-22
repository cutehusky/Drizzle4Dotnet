using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific DELETE query builder.
/// Extends DeleteQuery with:
/// - RETURNING clause
/// - DELETE ... USING (JOIN support via USING clause)
/// </summary>
public class PgDeleteQuery<TTable> : DeleteQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<PgSqlSqlDialectImpl>, IGenericSql?)> _usingTables = new();

    public PgDeleteQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds a RETURNING clause to capture deleted rows.
    /// </summary>
    public new ReturningQuery<TReturn, PgSqlSqlDialectImpl> Returning<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new ReturningQuery<TReturn, PgSqlSqlDialectImpl>(this, selectedColumns);
    }

    /// <summary>
    /// Adds a USING clause for DELETE ... USING joins.
    /// PostgreSQL syntax: DELETE FROM t USING other_t WHERE condition
    /// </summary>
    public PgDeleteQuery<TTable> Using(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql? joinCondition = null)
    {
        _usingTables.Add((table, joinCondition));
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("DELETE FROM ");
        _table.BuildRefSql(sqlBuilder);

        // PostgreSQL DELETE ... USING syntax
        if (_usingTables.Count > 0)
        {
            sqlBuilder.Append(" USING ");
            for (int i = 0; i < _usingTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _usingTables[i].Item1.BuildRefSql(sqlBuilder);
            }
        }

        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);
    }
}

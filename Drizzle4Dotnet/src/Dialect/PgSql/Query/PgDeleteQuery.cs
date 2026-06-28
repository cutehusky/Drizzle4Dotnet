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
public class PgDeleteQuery<TTable> : DeleteQuery<TTable, PgSqlSqlDialectImpl, PgDeleteQuery<TTable>>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private readonly List<IGenericTable<PgSqlSqlDialectImpl>> _usingTables = new();

    public PgDeleteQuery(TTable table, IQueryExecutor<PgSqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    /// <summary>
    /// Adds a USING clause for DELETE ... USING joins.
    /// PostgreSQL syntax: DELETE FROM t USING other_t WHERE condition
    /// </summary>
    public PgDeleteQuery<TTable> Using(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _usingTables.AddRange(tables);
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("DELETE FROM ");
        Table.BuildRefSql(sqlBuilder);

        // PostgreSQL DELETE ... USING syntax
        if (_usingTables.Count > 0)
        {
            sqlBuilder.Append(" USING ");
            for (int i = 0; i < _usingTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _usingTables[i].BuildRefSql(sqlBuilder);
            }
        }

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
}

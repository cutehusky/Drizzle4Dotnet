using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Sqlite;

/// <summary>
/// SQLite-specific DELETE query builder.
/// Extends DeleteQuery with:
/// - RETURNING clause
/// 
/// SQLite does NOT support:
/// - DELETE ... USING (no JOIN support — use correlated subqueries instead)
/// - LIMIT/OFFSET on DELETE
/// - ORDER BY on DELETE
/// </summary>
public class SqliteDeleteQuery<TTable> : DeleteQuery<TTable, SqliteSqlDialectImpl, SqliteDeleteQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteDeleteQuery(TTable table, IQueryExecutor<SqliteSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("DELETE FROM ");
        Table.BuildRefSql(sqlBuilder);

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
        
        // No USING clause, no ORDER BY, no LIMIT/OFFSET — not supported by SQLite for DELETE
    }
}

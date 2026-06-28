using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite.Query;

/// <summary>
/// SQLite-specific UPDATE query builder.
/// Extends UpdateQuery with:
/// - RETURNING clause
/// 
/// SQLite does NOT support:
/// - UPDATE ... FROM (no JOIN support — use correlated subqueries instead)
/// - LIMIT/OFFSET on UPDATE
/// - ORDER BY on UPDATE
/// </summary>
public class SqliteUpdateQuery<TTable> : UpdateQuery<TTable, SqliteSqlDialectImpl, SqliteUpdateQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteUpdateQuery(TTable table, IQueryExecutor<SqliteSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);
        
        SqlStatics.BuildSqlSetClause<SqliteSqlDialectImpl>(sqlBuilder, SetValues);

        // WHERE clause
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
        
        // No LIMIT/OFFSET — not supported by SQLite for UPDATE
        // No FROM clause — not supported by SQLite for UPDATE
    }
}

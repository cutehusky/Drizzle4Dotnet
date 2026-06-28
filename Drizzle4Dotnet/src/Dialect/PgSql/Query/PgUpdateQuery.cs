using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific UPDATE query builder.
/// Extends UpdateQuery with:
/// - RETURNING clause
/// - UPDATE ... FROM (JOIN support via FROM clause)
/// </summary>
public class PgUpdateQuery<TTable> : UpdateQuery<TTable, PgSqlSqlDialectImpl, PgUpdateQuery<TTable>>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private readonly List<IGenericTable<PgSqlSqlDialectImpl>> _fromTables = new();

    public PgUpdateQuery(TTable table, IQueryExecutor<PgSqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    /// <summary>
    /// Adds a FROM clause for UPDATE ... FROM joins.
    /// PostgreSQL syntax: UPDATE t SET ... FROM other_t WHERE condition
    /// </summary>
    public PgUpdateQuery<TTable> From(params IGenericTable<PgSqlSqlDialectImpl>[] tables)
    {
        _fromTables.AddRange(tables);
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);
        
        SqlStatics.BuildSqlSetClause<PgSqlSqlDialectImpl>(sqlBuilder, SetValues);

        // PostgreSQL UPDATE ... FROM syntax
        if (_fromTables.Count > 0)
        {
            sqlBuilder.Append(" FROM ");
            for (int i = 0; i < _fromTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _fromTables[i].BuildRefSql(sqlBuilder);
            }
        }

        // WHERE clause (includes join conditions if combined)
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
}

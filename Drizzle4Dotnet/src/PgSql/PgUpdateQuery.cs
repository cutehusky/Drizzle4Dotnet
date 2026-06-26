using Drizzle4Dotnet.Core;
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
    private readonly List<(IGenericTable<PgSqlSqlDialectImpl>, IGenericSql?)> _fromTables = new();

    public PgUpdateQuery(TTable table, IQueryExecutor<PgSqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    /// <summary>
    /// Adds a FROM clause for UPDATE ... FROM joins.
    /// PostgreSQL syntax: UPDATE t SET ... FROM other_t WHERE condition
    /// </summary>
    public PgUpdateQuery<TTable> From(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql? joinCondition = null)
    {
        _fromTables.Add((table, joinCondition));
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (SetValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }

        BuildSqlCte(sqlBuilder);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);

        // PostgreSQL UPDATE ... FROM syntax
        if (_fromTables.Count > 0)
        {
            sqlBuilder.Append(" FROM ");
            for (int i = 0; i < _fromTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _fromTables[i].Item1.BuildRefSql(sqlBuilder);
            }
        }

        BuildSqlSet(sqlBuilder, SetValues);

        // WHERE clause (includes join conditions if combined)
        AppendClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
}

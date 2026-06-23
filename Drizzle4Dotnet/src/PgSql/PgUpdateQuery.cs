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

    public PgUpdateQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
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
        if (_setValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }

        // CTE (WITH clause)
        if (_cteTables.Count > 0)
        {
            sqlBuilder.Append("WITH ");
            for (int i = 0; i < _cteTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _cteTables[i].BuildSql(sqlBuilder);
            }
            sqlBuilder.Append(' ');
        }

        sqlBuilder.Append("UPDATE ");
        _table.BuildRefSql(sqlBuilder);

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

        sqlBuilder.Append(" SET ");

        bool firstSet = true;
        foreach (var kv in _setValues)
        {
            if (!firstSet) sqlBuilder.Append(", ");

            sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = ");

            if (kv.Value is IGenericSql op)
            {
                sqlBuilder.Append('(');
                op.BuildSql(sqlBuilder);
                sqlBuilder.Append(')');
            }
            else
            {
                sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
            }
            firstSet = false;
        }

        // WHERE clause (includes join conditions if combined)
        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);
    }
}

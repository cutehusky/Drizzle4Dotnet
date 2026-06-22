using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific UPDATE query builder.
/// Extends UpdateQuery with MySQL-specific features:
/// - UPDATE with JOIN (MySQL syntax: UPDATE t1 JOIN t2 ON ... SET ... WHERE ...)
/// MySQL does not support RETURNING — use MySqlFunctions.RowCount() instead.
/// </summary>
public class MySqlUpdateQuery<TTable> : UpdateQuery<TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MySqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();

    public MySqlUpdateQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    // ====== MySQL UPDATE with JOIN ======
    
    private MySqlUpdateQuery<TTable> JoinInternal(
        IGenericTable<MySqlSqlDialectImpl> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MySqlUpdateQuery<TTable> InnerJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public MySqlUpdateQuery<TTable> LeftJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public MySqlUpdateQuery<TTable> RightJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public MySqlUpdateQuery<TTable> CrossJoin(IGenericTable<MySqlSqlDialectImpl> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_setValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }

        sqlBuilder.Append("UPDATE ");
        _table.BuildRefSql(sqlBuilder);

        // MySQL UPDATE JOIN syntax: UPDATE t1 JOIN t2 ON ... SET ...
        if (_joins.Count > 0)
        {
            foreach (var (table, type, on) in _joins)
            {
                sqlBuilder.Append(' ').Append(type).Append(" JOIN ");
                table.BuildRefSql(sqlBuilder);
                if (on != null)
                {
                    sqlBuilder.Append(" ON (");
                    on.BuildSql(sqlBuilder);
                    sqlBuilder.Append(')');
                }
            }
        }

        sqlBuilder.Append(" SET ");

        bool firstSet = true;
        foreach (var kv in _setValues)
        {
            if (!firstSet) sqlBuilder.Append(", ");

            sqlBuilder.Append(MySqlSqlDialectImpl.BuildIdentifier(kv.Key));
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

        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);
    }
}

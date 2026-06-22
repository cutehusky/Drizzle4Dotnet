using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific DELETE query builder.
/// Extends DeleteQuery with MySQL-specific features:
/// - DELETE with JOIN (MySQL syntax: DELETE t1 FROM t1 JOIN t2 ON ... WHERE ...)
/// - LIMIT and ORDER BY on DELETE
/// MySQL does not support RETURNING — use MySqlFunctions.RowCount() instead.
/// </summary>
public class MySqlDeleteQuery<TTable> : DeleteQuery<TTable, MySqlSqlDialectImpl, MySqlDeleteQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MySqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _limit;
    private readonly List<(IGenericSql, bool)> _orderBys = new();

    public MySqlDeleteQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    // ====== MySQL DELETE with JOIN ======
    
    private MySqlDeleteQuery<TTable> JoinInternal(
        IGenericTable<MySqlSqlDialectImpl> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MySqlDeleteQuery<TTable> InnerJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public MySqlDeleteQuery<TTable> LeftJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public MySqlDeleteQuery<TTable> RightJoin(IGenericTable<MySqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public MySqlDeleteQuery<TTable> CrossJoin(IGenericTable<MySqlSqlDialectImpl> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }

    // ====== MySQL LIMIT and ORDER BY on DELETE ======

    public MySqlDeleteQuery<TTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public MySqlDeleteQuery<TTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        // MySQL DELETE with JOIN syntax: DELETE t1 FROM t1 JOIN t2 ON ... WHERE ...
        sqlBuilder.Append("DELETE ");
        _table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" FROM ");
        _table.BuildRefSql(sqlBuilder);

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

        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);

        // ORDER BY (MySQL-specific on DELETE)
        if (_orderBys.Count > 0)
        {
            sqlBuilder.Append(" ORDER BY ");
            for (int i = 0; i < _orderBys.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                var (expr, isAsc) = _orderBys[i];
                expr.BuildSql(sqlBuilder);
                sqlBuilder.Append(isAsc ? " ASC" : " DESC");
            }
        }

        // LIMIT (MySQL-specific on DELETE)
        if (_limit.HasValue)
            sqlBuilder.Append(" LIMIT ").Append(sqlBuilder.AddParameter(_limit.Value));
    }
}

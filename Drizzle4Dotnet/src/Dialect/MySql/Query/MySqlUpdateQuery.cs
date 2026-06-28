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
/// - LIMIT and ORDER BY on UPDATE
/// MySQL does not support RETURNING — use MySqlFunctions.RowCount() instead.
/// </summary>
public class MySqlUpdateQuery<TTable> : UpdateQuery<TTable, MySqlSqlDialectImpl, MySqlUpdateQuery<TTable>>,
    ISupportOffsetLimit<MySqlUpdateQuery<TTable>>,
    ISupportOrderBy<MySqlUpdateQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MySqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _limit;
    private int? _offset;
    private readonly List<(IGenericSql, bool)> _orderBys = new();

    public MySqlUpdateQuery(TTable table, IQueryExecutor<MySqlSqlDialectImpl> executor) 
        : base(table, executor)
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

    // ====== MySQL LIMIT and ORDER BY on UPDATE ======

    public MySqlUpdateQuery<TTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public MySqlUpdateQuery<TTable> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    public MySqlUpdateQuery<TTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (SetValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);

        // MySQL UPDATE JOIN syntax
        SqlStatics.BuildSqlJoins<MySqlSqlDialectImpl>(sqlBuilder, _joins);

        SqlStatics.BuildSqlSetClause<MySqlSqlDialectImpl>(sqlBuilder, SetValues);

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // ORDER BY (MySQL-specific on UPDATE)
        SqlStatics.BuildSqlOrderBy(sqlBuilder, _orderBys);
        
        if (_offset.HasValue && !_limit.HasValue)
        {
            throw new InvalidOperationException("OFFSET cannot be used without LIMIT in MySQL DELETE.");
        }
        
        // LIMIT limit or LIMIT offset, limit (MySQL-specific on DELETE)
        if (_limit.HasValue)
        {
            if (_offset.HasValue)
            {
                sqlBuilder.Append(" LIMIT ");
                sqlBuilder.Append(sqlBuilder.AddParameter(_offset.Value));
                sqlBuilder.Append(", ");
                sqlBuilder.Append(sqlBuilder.AddParameter(_limit.Value));
            }
            else
            {
                sqlBuilder.Append(" LIMIT ");
                sqlBuilder.Append(sqlBuilder.AddParameter(_limit.Value));
            }
        }
    }
}

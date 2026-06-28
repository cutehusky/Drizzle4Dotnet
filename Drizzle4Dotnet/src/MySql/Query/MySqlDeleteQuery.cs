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
/// TODO: Support multi-table DELETE syntax: DELETE t1, t2 FROM t1 JOIN t2 ON ... WHERE ...
public class MySqlDeleteQuery<TTable> : DeleteQuery<TTable, MySqlSqlDialectImpl, MySqlDeleteQuery<TTable>>,
    ISupportOffsetLimit<MySqlDeleteQuery<TTable>>,
    ISupportOrderBy<MySqlDeleteQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MySqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _limit;
    private int? _offset;
    private readonly List<(IGenericSql, bool)> _orderBys = new();

    public MySqlDeleteQuery(TTable table, IQueryExecutor<MySqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ====== MySQL DELETE with JOIN ======
    
    private MySqlDeleteQuery<TTable> JoinInternal(
        IGenericTable<MySqlSqlDialectImpl> table,
        IGenericSql? on,
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
        => JoinInternal(table, null, "CROSS");

    // ====== MySQL-specific validation ======

    protected override void ValidateQuery()
    {
        base.ValidateQuery();
        if (_offset.HasValue && !_limit.HasValue)
        {
            throw new InvalidOperationException("OFFSET cannot be used without LIMIT in MySQL DELETE.");
        }
        if (_offset is < 0)
        {
            throw new InvalidOperationException("OFFSET cannot be negative in MySQL DELETE.");
        }
        if (_limit is <= 0)
        {
            throw new InvalidOperationException("LIMIT must be greater than zero in MySQL DELETE.");
        }
    }

    // ====== MySQL LIMIT and ORDER BY on DELETE ======

    public MySqlDeleteQuery<TTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public MySqlDeleteQuery<TTable> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    public MySqlDeleteQuery<TTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public MySqlDeleteQuery<TTable> OrderBy(params (IGenericSql col, bool asc)[] columns)
    {
        _orderBys.AddRange(columns);
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        if (_joins.Count > 0)
        {
            // MySQL DELETE with JOIN syntax: DELETE t1 FROM t1 JOIN t2 ON ... WHERE ...
            sqlBuilder.Append("DELETE ");
            if (Table is IMySqlTableAlias alias)
            {
                sqlBuilder.Append(alias.Alias);
            }
            else
            {
                Table.BuildRefSql(sqlBuilder);
            }
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            SqlStatics.BuildSqlJoins(sqlBuilder, _joins);
        }
        else
        {
            // Standard DELETE: DELETE FROM t WHERE ...
            sqlBuilder.Append("DELETE FROM ");
            Table.BuildRefSql(sqlBuilder);
        }

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

        // ORDER BY (MySQL-specific on DELETE)
        SqlStatics.BuildSqlOrderBy(sqlBuilder, _orderBys);
        
        // LIMIT & OFFSET (MySQL-specific syntax via dialect)
        MySqlSqlDialectImpl.BuildLimitOffsetForUpdateDelete(sqlBuilder, _limit, _offset);
    }
}

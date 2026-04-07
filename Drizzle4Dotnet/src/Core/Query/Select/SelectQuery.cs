using System.Text;
using Drizzle4Dotnet.Core.Query.Shared.Operators;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Select;

public enum ELockType
{
    None,
    ForUpdate,
    ForShare,
    ForNoKeyUpdate,
    ForKeyShare
}

public class SelectQuery<TReturn, TDialect>: Query<TReturn, TDialect> where TDialect : ISqlDialect
{
    private string? _from;
    private readonly List<(ITableType<TDialect>, string, IOperator?)> _joins = new();
    private readonly List<IOperator> _wheres = new();
    private readonly List<(IColumnType<TDialect>, bool)> _orderBys = new();
    private int? _limit;
    private int? _offset;
    private bool _distinct;
    private readonly List<IColumnType<TDialect>> _groupBys = new();
    private readonly List<IOperator> _havings = new();
    private string? _lockClause;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }

    public override string Sql
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            if (_distinct) sb.Append("DISTINCT ");
            sb.Append(SelectedColumns.Sql);

            // FROM
            if (_from != null)
            {
                sb.Append(" FROM ");
                sb.Append(_from);
            }

            // JOIN
            var joins = _joins.Select(j =>
            {
                var (table, type, on) = j;
                var joinSql = $"{type} JOIN {table.Sql}";
                if (on != null)
                {
                    joinSql += $" ON {on.BuildSql(Parameters)}";
                }
                return joinSql;
            }).ToList();
            if (joins.Count > 0)
            {
                sb.Append(" ");
                sb.Append(string.Join(" ", joins));
            }

            // WHERE
            var wheres = _wheres.Select(w => w.BuildSql(Parameters)).ToList();
            if (wheres.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", wheres));
            }
            
            var groupBys = _groupBys.Select(g => g.Sql).ToList();
            if (groupBys.Count > 0)
            {
                sb.Append(" GROUP BY ");
                sb.Append(string.Join(", ", groupBys));
            }

            var havings = _havings.Select(h => h.BuildSql(Parameters)).ToList();
            if (havings.Count > 0)
            {
                sb.Append(" HAVING ");
                sb.Append(string.Join(" AND ", havings));
            }

            // ORDER BY
            var orderBys = _orderBys.Select(o => $"{o.Item1.Sql} {(o.Item2 ? "ASC" : "DESC")}").ToList();
            if (orderBys.Count > 0)
            {
                sb.Append(" ORDER BY ");
                sb.Append(string.Join(", ", orderBys));
            }

            // LIMIT
            if (_limit.HasValue)
            {
                sb.Append(" LIMIT ");
                sb.Append(_limit.Value);
            }

            // OFFSET
            if (_offset.HasValue)
            {
                sb.Append(" OFFSET ");
                sb.Append(_offset.Value);
            }
            
            if (_lockClause != null) sb.Append($" {_lockClause}");

            return sb.ToString();
        }
    }
    
    
    public SelectQuery<TReturn, TDialect> From(ITable<TDialect> table)
    {
        _from = table.Sql;
        return this;
    }

    public SelectQuery<TReturn, TDialect> Where(IOperator condition)
    {
        _wheres.Add(condition);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Where(params IOperator[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy(IColumnType<TDialect> columns)
    {
        _groupBys.Add(columns);
        return this;
    }

    public SelectQuery<TReturn, TDialect> Having(IOperator condition)
    {
        _havings.Add(condition);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Having(params IOperator[] conditions)
    {
        _havings.AddRange(conditions);
        return this;
    }

    public SelectQuery<TReturn, TDialect> OrderBy(IColumnType<TDialect> col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public SelectQuery<TReturn, TDialect> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public SelectQuery<TReturn, TDialect> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // ====== JOINS ======
    private SelectQuery<TReturn, TDialect> JoinInternal(
        ITableType<TDialect> table,
        IOperator on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public SelectQuery<TReturn, TDialect> InnerJoin(ITableType<TDialect> table, IOperator on)
        => JoinInternal(table, on, "INNER");

    public SelectQuery<TReturn, TDialect> LeftJoin(ITableType<TDialect> table, IOperator on)
        => JoinInternal(table, on, "LEFT");

    public SelectQuery<TReturn, TDialect> RightJoin(ITableType<TDialect> table, IOperator on)
        => JoinInternal(table, on, "RIGHT");

    public SelectQuery<TReturn, TDialect> FullJoin(ITableType<TDialect> table, IOperator on)
        => JoinInternal(table, on, "FULL");

    public SelectQuery<TReturn, TDialect> CrossJoin(ITableType<TDialect> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Distinct()
    {
        _distinct = true;
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> ForUpdate() { _lockClause = "FOR UPDATE"; return this; }
    public SelectQuery<TReturn, TDialect> ForShare() { _lockClause = "FOR SHARE"; return this; }
    public SelectQuery<TReturn, TDialect> ForNoKeyUpdate() { _lockClause = "FOR NO KEY UPDATE"; return this; }
    public SelectQuery<TReturn, TDialect> ForKeyShare() { _lockClause = "FOR KEY SHARE"; return this; }
    public SelectQuery<TReturn, TDialect> For(ELockType lockType)
    {
        _lockClause = lockType switch
        {
            ELockType.ForUpdate => "FOR UPDATE",
            ELockType.ForShare => "FOR SHARE",
            ELockType.ForNoKeyUpdate => "FOR NO KEY UPDATE",
            ELockType.ForKeyShare => "FOR KEY SHARE",
            _ => null
        };
        return this;
    }
}
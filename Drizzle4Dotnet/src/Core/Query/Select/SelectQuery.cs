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
    private readonly List<string> _joins = new();
    private readonly List<string> _wheres = new();
    private readonly List<string> _orderBys = new();
    private int? _limit;
    private int? _offset;
    private bool _distinct;
    private readonly List<string> _groupBys = new();
    private readonly List<string> _havings = new();
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
            if (_joins.Count > 0)
            {
                sb.Append(" ");
                sb.Append(string.Join(" ", _joins));
            }

            // WHERE
            if (_wheres.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", _wheres));
            }
            
            if (_groupBys.Count > 0)
            {
                sb.Append(" GROUP BY ");
                sb.Append(string.Join(", ", _groupBys));
            }

            if (_havings.Count > 0)
            {
                sb.Append(" HAVING ");
                sb.Append(string.Join(" AND ", _havings));
            }

            // ORDER BY
            if (_orderBys.Count > 0)
            {
                sb.Append(" ORDER BY ");
                sb.Append(string.Join(", ", _orderBys));
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
        var sql = condition.BuildSql(Parameters);
        _wheres.Add(sql);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Where(params IOperator[] conditions)
    {
        foreach (var condition in conditions)
        {
            var sql = condition.BuildSql(Parameters);
            _wheres.Add(sql);
        }
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy<TCol>(IColumn<TCol> columns)
    {
        _groupBys.Add(columns.Sql);
        return this;
    }

    public SelectQuery<TReturn, TDialect> Having(IOperator condition)
    {
        _havings.Add(condition.BuildSql(Parameters));
        return this;
    }
    
    
    public SelectQuery<TReturn, TDialect> Having(params IOperator[] conditions)
    {
        foreach (var condition in conditions)
        {
            var sql = condition.BuildSql(Parameters);
            _havings.Add(sql);
        }
        return this;
    }

    public SelectQuery<TReturn, TDialect> OrderBy<TCol>(ISql<TCol> col, bool asc = true)
    {
        _orderBys.Add($"{col.Sql} {(asc ? "ASC" : "DESC")}");
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
        ITable<TDialect> table,
        IOperator on,
        string type)
    {
        var sql = $"{type} JOIN {table.Sql} ON {on.BuildSql(Parameters)}";
        _joins.Add(sql);
        return this;
    }

    public SelectQuery<TReturn, TDialect> InnerJoin(ITable<TDialect> table, IOperator on)
        => JoinInternal(table, on, "INNER");

    public SelectQuery<TReturn, TDialect> LeftJoin(ITable<TDialect> table, IOperator on)
        => JoinInternal(table, on, "LEFT");

    public SelectQuery<TReturn, TDialect> RightJoin(ITable<TDialect> table, IOperator on)
        => JoinInternal(table, on, "RIGHT");

    public SelectQuery<TReturn, TDialect> FullJoin(ITable<TDialect> table, IOperator on)
        => JoinInternal(table, on, "FULL");

    public SelectQuery<TReturn, TDialect> CrossJoin(ITable<TDialect> table)
    {
        _joins.Add($"CROSS JOIN {table.Sql}");
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
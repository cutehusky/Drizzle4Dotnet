using System.Text;
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
    private IGenericTable<TDialect>? _from;
    private readonly List<(IGenericTable<TDialect>, string, IGenericSql?)> _joins = new();
    private readonly List<IGenericSql> _wheres = new();
    private readonly List<(IGenericSql, bool)> _orderBys = new();
    private int? _limit;
    private int? _offset;
    private bool _distinct;
    private readonly List<IGenericSql> _groupBys = new();
    private readonly List<IGenericSql> _havings = new();
    private string? _lockClause;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public override void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        sb.Append("SELECT ");
        if (_distinct) sb.Append("DISTINCT ");
        SelectedColumns.BuildSql(parameters, sb);

        // FROM
        if (_from != null)
        {
            sb.Append(" FROM ");
            _from.BuildSql(parameters, sb);
        }

        if (_joins.Count > 0)
        {
            foreach (var (table, type, on) in _joins)
            {
                sb.Append(' ').Append(type).Append(" JOIN ");
                table.BuildSql(parameters, sb);
                if (on != null)
                {
                    sb.Append(" ON (");
                    on.BuildSql(parameters, sb);
                    sb.Append(')');
                }
            }
        }

        // WHERE
        AppendClause(sb, " WHERE ", " AND ", _wheres, parameters, wrapInParentheses: true);

        // GROUP BY
        AppendClause(sb, " GROUP BY ", ", ", _groupBys, parameters);

        // HAVING
        AppendClause(sb, " HAVING ", " AND ", _havings, parameters, wrapInParentheses: true);

        // ORDER BY
        if (_orderBys.Count > 0)
        {
            sb.Append(" ORDER BY ");
            for (int i = 0; i < _orderBys.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var (expr, isAsc) = _orderBys[i];
                expr.BuildSql(parameters, sb);
                sb.Append(isAsc ? " ASC" : " DESC");
            }
        }

        // LIMIT & OFFSET
        if (_limit.HasValue) sb.Append(" LIMIT ").Append(_limit.Value);
        if (_offset.HasValue) sb.Append(" OFFSET ").Append(_offset.Value);
        
        if (_lockClause != null) sb.Append(' ').Append(_lockClause);
    }
    
    public override string BuildSql(Dictionary<string, object?> parameters)
    {
        var sb = new StringBuilder(); 
        BuildSql(parameters, sb);
        return sb.ToString();
    }

    public SelectQuery<TReturn, TDialect> From(IGenericTable<TDialect> table)
    {
        _from = table;
        return this;
    }

    
    public SelectQuery<TReturn, TDialect> Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Where(IGenericSql conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy(IGenericSql columns)
    {
        _groupBys.Add(columns);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> GroupBy(params IGenericSql[] columns)
    {
        _groupBys.AddRange(columns);
        return this;
    }

    public SelectQuery<TReturn, TDialect> Having(IGenericSql condition)
    {
        _havings.Add(condition);
        return this;
    }
    
    public SelectQuery<TReturn, TDialect> Having(params IGenericSql[] conditions)
    {
        _havings.AddRange(conditions);
        return this;
    }

    public SelectQuery<TReturn, TDialect> OrderBy(IGenericSql col, bool asc = true)
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
        IGenericTable<TDialect> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public SelectQuery<TReturn, TDialect> InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public SelectQuery<TReturn, TDialect> LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public SelectQuery<TReturn, TDialect> RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public SelectQuery<TReturn, TDialect> FullJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    public SelectQuery<TReturn, TDialect> CrossJoin(IGenericTable<TDialect> table)
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

public class SelectQuery<TReturn, TDialect, TVirtualTable>: Query<TReturn, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    private IGenericTable<TDialect>? _from;
    private readonly List<(IGenericTable<TDialect>, string, IGenericSql?)> _joins = new();
    private readonly List<IGenericSql> _wheres = new();
    private readonly List<(IGenericSql, bool)> _orderBys = new();
    private int? _limit;
    private int? _offset;
    private bool _distinct;
    private readonly List<IGenericSql> _groupBys = new();
    private readonly List<IGenericSql> _havings = new();
    private string? _lockClause;

    public SelectQuery(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(selectedColumns, dbClient)
    {
    }
    
    public override void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        sb.Append("SELECT ");
        if (_distinct) sb.Append("DISTINCT ");
        SelectedColumns.BuildSql(parameters, sb);

        // FROM
        if (_from != null)
        {
            sb.Append(" FROM ");
            _from.BuildSql(parameters, sb);
        }

        if (_joins.Count > 0)
        {
            foreach (var (table, type, on) in _joins)
            {
                sb.Append(' ').Append(type).Append(" JOIN ");
                table.BuildSql(parameters, sb);
                if (on != null)
                {
                    sb.Append(" ON (");
                    on.BuildSql(parameters, sb);
                    sb.Append(')');
                }
            }
        }

        // WHERE
        AppendClause(sb, " WHERE ", " AND ", _wheres, parameters, wrapInParentheses: true);

        // GROUP BY
        AppendClause(sb, " GROUP BY ", ", ", _groupBys, parameters);

        // HAVING
        AppendClause(sb, " HAVING ", " AND ", _havings, parameters, wrapInParentheses: true);

        // ORDER BY
        if (_orderBys.Count > 0)
        {
            sb.Append(" ORDER BY ");
            for (int i = 0; i < _orderBys.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var (expr, isAsc) = _orderBys[i];
                expr.BuildSql(parameters, sb);
                sb.Append(isAsc ? " ASC" : " DESC");
            }
        }

        // LIMIT & OFFSET
        if (_limit.HasValue) sb.Append(" LIMIT ").Append(_limit.Value);
        if (_offset.HasValue) sb.Append(" OFFSET ").Append(_offset.Value);
        
        if (_lockClause != null) sb.Append(' ').Append(_lockClause);
    }
    
    public override string BuildSql(Dictionary<string, object?> parameters)
    {
        var sb = new StringBuilder(); 
        BuildSql(parameters, sb);
        return sb.ToString();
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> From(IGenericTable<TDialect> table)
    {
        _from = table;
        return this;
    }

    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Where(IGenericSql conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> GroupBy(IGenericSql columns)
    {
        _groupBys.Add(columns);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> GroupBy(params IGenericSql[] columns)
    {
        _groupBys.AddRange(columns);
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Having(IGenericSql condition)
    {
        _havings.Add(condition);
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Having(params IGenericSql[] conditions)
    {
        _havings.AddRange(conditions);
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> OrderBy(IGenericSql col, bool asc = true)
    {
        _orderBys.Add((col, asc));
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    // ====== JOINS ======
    private  SelectQuery<TReturn, TDialect, TVirtualTable> JoinInternal(
        IGenericTable<TDialect> table,
        IGenericSql on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public  SelectQuery<TReturn, TDialect, TVirtualTable> InnerJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> LeftJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> RightJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> FullJoin(IGenericTable<TDialect> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    public  SelectQuery<TReturn, TDialect, TVirtualTable> CrossJoin(IGenericTable<TDialect> table)
    {
        _joins.Add((table, "CROSS", null));
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> Distinct()
    {
        _distinct = true;
        return this;
    }
    
    public  SelectQuery<TReturn, TDialect, TVirtualTable> ForUpdate() { _lockClause = "FOR UPDATE"; return this; }
    public  SelectQuery<TReturn, TDialect, TVirtualTable> ForShare() { _lockClause = "FOR SHARE"; return this; }
    public  SelectQuery<TReturn, TDialect, TVirtualTable> ForNoKeyUpdate() { _lockClause = "FOR NO KEY UPDATE"; return this; }
    public  SelectQuery<TReturn, TDialect, TVirtualTable> ForKeyShare() { _lockClause = "FOR KEY SHARE"; return this; }
    public  SelectQuery<TReturn, TDialect, TVirtualTable> For(ELockType lockType)
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
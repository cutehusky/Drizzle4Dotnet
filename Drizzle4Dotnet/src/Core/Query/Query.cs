using System.Runtime.CompilerServices;
using System.Text;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query<TDialect>: ISql where TDialect : ISqlDialect
{
    public readonly DbClient<TDialect> DbClient;

    public Query(DbClient<TDialect> dbClient)
    {
        DbClient = dbClient;
    }
    
    public TaskAwaiter GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public abstract string BuildSql(Dictionary<string, object?> parameters);
    
    public (string, Dictionary<string, object?>) Build()
    {
        var parameters = new Dictionary<string, object?>();
        var sql = BuildSql(parameters);
        return (sql, parameters);
    }
    
    
    protected void AppendClause(StringBuilder sb, string header, string separator, IReadOnlyList<ISql> items, 
        Dictionary<string, object?> parameters, bool wrapInParentheses = false)
    {
        if (items.Count == 0) return;

        sb.Append(header);
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(separator);
            if (wrapInParentheses) sb.Append('(');
            sb.Append(items[i].BuildSql(parameters));
            if (wrapInParentheses) sb.Append(')');
        }
    }
    
    public ReturningQuery<TReturn, TDialect> Returning<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new ReturningQuery<TReturn, TDialect>(this, selectedColumns);
    }
}


public abstract class Query<TReturn, TDialect>: Query<TDialect>, IReturning<TReturn, TDialect> where TDialect : ISqlDialect
{
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public Query(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(dbClient)
    {
        SelectedColumns = selectedColumns;
    }
    
    public new TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }
}

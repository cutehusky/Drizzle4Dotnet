using System.Runtime.CompilerServices;
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
}


public abstract class Query<TReturn, TDialect>: IReturning<TReturn, TDialect> where TDialect : ISqlDialect
{
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }
    public readonly DbClient<TDialect> DbClient;

    public Query(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        )
    {
        SelectedColumns = selectedColumns;
        DbClient = dbClient;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
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
}

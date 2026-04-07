using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query<TDialect>: IParameterizedSql where TDialect : ISqlDialect
{
    public readonly DbClient<TDialect> DbClient;
    public Dictionary<string, object?> Parameters { get; } 

    public Query(DbClient<TDialect> dbClient)
    {
        DbClient = dbClient;
        Parameters = new Dictionary<string, object?>();
    }
    
    public TaskAwaiter GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public abstract string Sql { get; }
}


public abstract class Query<TReturn, TDialect>: IParameterizedSql<TReturn, TDialect> where TDialect : ISqlDialect
{
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }
    public readonly DbClient<TDialect> DbClient;
    public Dictionary<string, object?> Parameters { get; }

    public Query(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        )
    {
        SelectedColumns = selectedColumns;
        DbClient = dbClient;
        Parameters = new Dictionary<string, object?>();
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public abstract string Sql { get; }
}

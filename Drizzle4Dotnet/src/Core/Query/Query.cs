using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query: IParameterizedSql
{
    public readonly DbClient DbClient;
    public Dictionary<string, object?> Parameters { get; } 

    public Query(DbClient dbClient)
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


public abstract class Query<TReturn>: IParameterizedSql<TReturn>
{
    public ISelectedColumns<TReturn> SelectedColumns { get; }
    public readonly DbClient DbClient;
    public Dictionary<string, object?> Parameters { get; }

    public Query(
        ISelectedColumns<TReturn> selectedColumns,
        DbClient dbClient
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

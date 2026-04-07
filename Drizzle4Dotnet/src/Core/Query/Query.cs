using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Query.Select;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query<TReturn>: IReturning<TReturn>
{
    protected readonly DbClient DbClient;
    public Dictionary<string, object?> Parameters { get; } 
    public ISelectedColumns<TReturn>? SelectedColumns { get; set; }

    public Query(
        ISelectedColumns<TReturn>? selectedColumns,
        DbClient dbClient)
    {
        SelectedColumns = selectedColumns;
        Parameters = new Dictionary<string, object?>();
        DbClient = dbClient;
    }
    
    public Query(
        ISelectedColumns<TReturn> selectedColumns,
        Dictionary <string, object?> parameters,
        DbClient dbClient)
    {
        SelectedColumns = selectedColumns;
        Parameters = parameters;
        DbClient = dbClient;
    }

    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public abstract string Sql { get; }
}


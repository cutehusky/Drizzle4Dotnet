using System.Runtime.CompilerServices;

namespace Drizzle4Dotnet.Core.Query.Select;


public class ReturningQuery<TReturn> : IParameterizedSql<TReturn>
{
    private readonly Query _baseQuery;
    public readonly DbClient DbClient;
    public Dictionary<string, object?> Parameters { get; }
    public ISelectedColumns<TReturn> SelectedColumns { get; }

    public ReturningQuery(
        Query baseQuery,
        ISelectedColumns<TReturn> selectedColumns
        )
    {
        _baseQuery = baseQuery;
        SelectedColumns = selectedColumns;
        DbClient = baseQuery.DbClient;
        Parameters = baseQuery.Parameters;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public string Sql => $"{_baseQuery.Sql} RETURNING {SelectedColumns!.Sql}";
}

using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public class ReturningQuery<TReturn, TDialect> : IParameterizedSql<TReturn, TDialect> where  TDialect : ISqlDialect
{
    private readonly Query<TDialect> _baseQuery;
    public readonly DbClient<TDialect> DbClient;
    public Dictionary<string, object?> Parameters { get; }
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public ReturningQuery(
        Query<TDialect> baseQuery,
        ISelectedColumns<TReturn, TDialect> selectedColumns
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

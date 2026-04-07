using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public class ReturningQuery<TReturn, TDialect> : IReturning<TReturn, TDialect> where  TDialect : ISqlDialect
{
    private readonly Query<TDialect> _baseQuery;
    public readonly DbClient<TDialect> DbClient;
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public ReturningQuery(
        Query<TDialect> baseQuery,
        ISelectedColumns<TReturn, TDialect> selectedColumns
        )
    {
        _baseQuery = baseQuery;
        SelectedColumns = selectedColumns;
        DbClient = baseQuery.DbClient;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"{_baseQuery.BuildSql(parameters)} RETURNING {SelectedColumns.Sql}";
    }
    
    public (string, Dictionary<string, object?>) Build()
    {
        var parameters = new Dictionary<string, object?>();
        var sql = BuildSql(parameters);
        return (sql, parameters);
    }
}

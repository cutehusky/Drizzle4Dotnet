using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public class ReturningQuery<TReturn, TDialect> : QueryBase<TDialect>, IReturning<TReturn, TDialect> where  TDialect : ISqlDialect
{
    private readonly Query<TDialect> _baseQuery;
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public ReturningQuery(
        Query<TDialect> baseQuery,
        ISelectedColumns<TReturn, TDialect> selectedColumns
        ): base(baseQuery.DbClient)
    {
        _baseQuery = baseQuery;
        SelectedColumns = selectedColumns;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteGetListAsync(this).GetAwaiter();
    }

    public override string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"{_baseQuery.BuildSql(parameters)} RETURNING {SelectedColumns.BuildSql(parameters)}";
    }
}

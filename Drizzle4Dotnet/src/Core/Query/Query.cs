using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query<TDialect>: QueryBase<TDialect>, ISql where TDialect : ISqlDialect
{
    public Query(DbClient<TDialect> dbClient)
        : base(dbClient)
    {
    }
    
    public TaskAwaiter GetAwaiter()
    {
        return DbClient.ExecuteAsync(this).GetAwaiter();
    }
    
    public ReturningQuery<TReturn, TDialect> Returning<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new ReturningQuery<TReturn, TDialect>(this, selectedColumns);
    }
}


public abstract class Query<TReturn, TDialect>: QueryBase<TDialect>, IReturning<TReturn, TDialect> where TDialect : ISqlDialect
{
    public ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }

    public Query(
        ISelectedColumns<TReturn, TDialect> selectedColumns,
        DbClient<TDialect> dbClient
        ): base(dbClient)
    {
        SelectedColumns = selectedColumns;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return DbClient.ExecuteGetListAsync(this).GetAwaiter();
    }
}

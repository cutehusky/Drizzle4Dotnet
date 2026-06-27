using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public abstract class Query<TDialect>: QueryBase<TDialect>, ISql where TDialect : ISqlDialect
{
    public Query(IQueryExecutor<TDialect> executor)
        : base(executor)
    {
    }
    
    public TaskAwaiter GetAwaiter()
    {
        return Executor.ExecuteAsync(this).GetAwaiter();
    }
    
    public ReturningQuery<TReturn, TDialect, TVirtualTable> Returning<TReturn, TVirtualTable>(ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<TDialect>
    {
        return new ReturningQuery<TReturn, TDialect, TVirtualTable>(this, selectedColumns);
    }
}

public abstract class Query<TReturn, TDialect, TVirtualTable>: QueryBase<TDialect>, IReturning<TReturn, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    public ISelectedColumns<TReturn, TDialect, TVirtualTable> SelectedColumns { get; }

    public Query(
        ISelectedColumns<TReturn, TDialect, TVirtualTable> selectedColumns,
        IQueryExecutor<TDialect> executor
    ): base(executor)
    {
        SelectedColumns = selectedColumns;
    }
    
    public TaskAwaiter<List<TReturn>> GetAwaiter()
    {
        return Executor.ExecuteGetListAsync(this).GetAwaiter();
    }
    
    public TVirtualTable AsSubQuery(string alias)
    {
        return (TVirtualTable) TVirtualTable.Create(this, alias, SelectedColumns);
    }
    
    public TypedTupleAnonymousGeneratedSubqueryTable<
        T, TReturn, TDialect
    > AsSubQuery<T>(string alias,
        Func<IGetFieldByName, T> columnSelector
    )
    {
        return new TypedTupleAnonymousGeneratedSubqueryTable<T, TReturn, TDialect>(
            alias,
            this,
            (ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn,TDialect>>) SelectedColumns,
            columnSelector
        );
    }
}

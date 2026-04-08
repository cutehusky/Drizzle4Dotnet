using System.Data.Common;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core;

public interface IQueryBuilder<TDialect> where TDialect : ISqlDialect
{
    public SelectQuery<TReturn, TDialect> Select<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns);

    public UpdateQuery<TTable, TDialect> Update<TTable>(TTable table) where TTable : ITable<TDialect>;

    public InsertQuery<TTable, TDialect> Insert<TTable>(TTable table) where TTable : ITable<TDialect>;
    public DeleteQuery<TTable, TDialect> Delete<TTable>(TTable table) where TTable : ITable<TDialect>;

    public SelectQuery<TReturn, TDialect> SelectDistinct<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns);
}

public class QueryBuilder<TDialect> : IQueryBuilder<TDialect> where TDialect : ISqlDialect
{
    public SelectQuery<TReturn, TDialect> Select<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new SelectQuery<TReturn, TDialect>(selectedColumns, null);
    }
    
    public UpdateQuery<TTable, TDialect> Update<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new UpdateQuery<TTable, TDialect>(table, null);
    }
    
    public InsertQuery<TTable, TDialect> Insert<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new InsertQuery<TTable, TDialect>(table, null);
    }
    
    public DeleteQuery<TTable, TDialect> Delete<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new DeleteQuery<TTable, TDialect>(table, null);
    }
    
    public SelectQuery<TReturn, TDialect> SelectDistinct<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new SelectQuery<TReturn, TDialect>(selectedColumns, null).Distinct();
    }
}

public sealed class DbClient<TDialect>: IQueryBuilder<TDialect>, IAsyncDisposable where TDialect : ISqlDialect
{
    private readonly DbConnection _conn;
    private readonly DbTransaction? _transaction;

    public DbClient(DbConnection conn,  DbTransaction? transaction = null)
    {
        _conn = conn;
        _transaction = transaction;
    }
    
    public async Task<DbClient<TDialect>> BeginTransactionAsync()
    {
        var transaction = await _conn.BeginTransactionAsync();
        return new DbClient<TDialect>(_conn, transaction);
    }

    public async Task CommitAsync() => await (_transaction?.CommitAsync() ?? Task.CompletedTask);
    public async Task RollbackAsync() => await (_transaction?.RollbackAsync() ?? Task.CompletedTask);


    public async Task<List<T>> ExecuteAsync<T>(IReturning<T, TDialect> query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var parameters = new Dictionary<string, object?>();
        cmd.CommandText = query.BuildSql(parameters);

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<T>();
        var mapper = query.Mapper;
        while (await reader.ReadAsync())
        {
            result.Add(mapper(reader));
        }
        return result;
    }
    
    public async Task ExecuteAsync(ISql query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var parameters = new Dictionary<string, object?>();
        cmd.CommandText = query.BuildSql(parameters);

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await cmd.ExecuteNonQueryAsync();
    }
    
    public SelectQuery<TReturn, TDialect> Select<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new SelectQuery<TReturn, TDialect>(selectedColumns, this);
    }
    
    public SelectQuery<TReturn, TDialect> SelectDistinct<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new SelectQuery<TReturn, TDialect>(selectedColumns, this).Distinct();
    }
    
    public UpdateQuery<TTable, TDialect> Update<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new UpdateQuery<TTable, TDialect>(table, this);
    }
    
    public InsertQuery<TTable, TDialect> Insert<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new InsertQuery<TTable, TDialect>(table, this);
    }
    
    public DeleteQuery<TTable, TDialect> Delete<TTable>(TTable table) where TTable : ITable<TDialect>
    {
        return new DeleteQuery<TTable, TDialect>(table, this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }
    
    public async Task RunInTransactionAsync(Func<DbClient<TDialect>, Task> action)
    {
        await using var txClient = await BeginTransactionAsync();
        try
        {
            await action(txClient);
            await txClient.CommitAsync();
        }
        catch
        {
            await txClient.RollbackAsync();
            throw;
        }
    }
}

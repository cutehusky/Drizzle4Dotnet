using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core;


public abstract class DbClient<TDialect>: IAsyncDisposable where TDialect : ISqlDialect
{
    protected readonly DbConnection _conn;
    protected readonly DbTransaction? _transaction;

    protected DbClient(DbConnection conn,  DbTransaction? transaction = null)
    {
        _conn = conn;
        _transaction = transaction;
    }

    public async Task<List<T>> ExecuteGetListAsync<T, TVirtualTable>(IReturning<T, TDialect, TVirtualTable> query) where TVirtualTable : IVirtualTable<TDialect>
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

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
    
    public async Task<List<T>> ExecuteGetListAsync<T>(IReturning<T, TDialect> query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

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

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await cmd.ExecuteNonQueryAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }
}


public abstract class DbClientWithTransaction<TInstance, TDialect>: DbClient<TDialect> 
where TDialect : ISqlDialect
where TInstance : DbClientWithTransaction<TInstance, TDialect>
{
    protected DbClientWithTransaction(DbConnection conn, DbTransaction? transaction = null) : base(conn, transaction)
    {
    }

    /// <summary>
    /// Factory method for subclasses to create a new instance of themselves with a transaction.
    /// Each concrete subclass must implement this to return its own type.
    /// </summary>
    protected abstract TInstance CreateInstance(DbConnection conn, DbTransaction? transaction);

    
    public async Task CommitAsync() => await (_transaction?.CommitAsync() ?? Task.CompletedTask);
    public async Task RollbackAsync() => await (_transaction?.RollbackAsync() ?? Task.CompletedTask);

    public async Task<TInstance> BeginTransactionAsync()
    {
        var transaction = await _conn.BeginTransactionAsync();
        return CreateInstance(_conn, transaction);
    }
    
    public async Task RunInTransactionAsync(Func<TInstance, Task> action)
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

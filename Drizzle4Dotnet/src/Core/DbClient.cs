using System.Data.Common;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core;


public sealed class DbClient<TDialect> where TDialect : ISqlDialect
{
    private readonly DbConnection _conn;

    public DbClient(DbConnection conn)
    {
        _conn = conn;
    }

    public async Task<List<T>> ExecuteAsync<T>(IReturning<T, TDialect> query)
    {
        await using var cmd = _conn.CreateCommand();
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
    
    public async Task ExecuteAsync(IParameterizedSql query)
    {
        await using var cmd = _conn.CreateCommand();
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
    }
    
    public SelectQuery<TReturn, TDialect> Select<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new SelectQuery<TReturn, TDialect>(selectedColumns, this);
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
}
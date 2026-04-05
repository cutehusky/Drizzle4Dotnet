using System.Data.Common;
using Drizzle4Dotnet.Query.Delete;
using Drizzle4Dotnet.Query.Insert;
using Drizzle4Dotnet.Query.Select;
using Drizzle4Dotnet.Query.Update;
using Drizzle4Dotnet.Schema.Tables;

namespace Drizzle4Dotnet;


public sealed class DbClient
{
    private readonly DbConnection _conn;

    public DbClient(DbConnection conn)
    {
        _conn = conn;
    }

    public async Task<List<T>> ExecuteAsync<T>(IReturning<T> query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.CommandText = query.Sql;

        foreach (var entry in query.Parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<T>();
        var mapper = query.Mapper;
        if (mapper == null)
            return result;
        while (await reader.ReadAsync())
        {
                
            result.Add(mapper(reader));
        }
        return result;
    }
    
    public SelectQuery<T> Select<T>(ISelectedColumns<T> selectedColumns)
    {
        return new SelectQuery<T>(selectedColumns, this);
    }
    
    public UpdateQuery<TTable> Update<TTable>(TTable table) where TTable : ITable
    {
        return new UpdateQuery<TTable>(table, this);
    }
    
    public InsertQuery<TTable> Insert<TTable>(TTable table) where TTable : ITable
    {
        return new InsertQuery<TTable>(table, this);
    }
    
    public DeleteQuery<TTable> Delete<TTable>(TTable table) where TTable : ITable
    {
        return new DeleteQuery<TTable>(table, this);
    }
}
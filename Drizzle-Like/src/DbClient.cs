using System.Data.Common;
using Drizzle_Like.Query.Delete;
using Drizzle_Like.Query.Insert;
using Drizzle_Like.Query.Select;
using Drizzle_Like.Query.Update;
using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like;


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
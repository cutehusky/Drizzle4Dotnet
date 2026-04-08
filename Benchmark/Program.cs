using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dapper;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Dialect;
using Microsoft.EntityFrameworkCore;
using MyNamespace;
using Npgsql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

[MemoryDiagnoser]
public class OrmBenchmark
{
    private NpgsqlConnection _connection;
    private DbClient<PgSqlSqlDialectImpl> _client;
    private AppDbContext _efContext;

    public static readonly UsersTable users = new UsersTable();
    public SelectQuery<UserSelect.SelectResult, PgSqlSqlDialectImpl> query;
    
    [GlobalSetup]
    public void Setup()
    {
        var connString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        
        var builder = new NpgsqlSlimDataSourceBuilder(connString);
        var dataSource = builder.Build();
        _connection = dataSource.OpenConnection(); 
        
        _client = new DbClient<PgSqlSqlDialectImpl>(_connection); 
        query = _client.Select(UserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1));
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connection)
            .Options;
        _efContext = new AppDbContext(options);

        _ = _efContext.Users?.AsNoTracking()?.FirstOrDefault(u => u.Id == 1);
    }

    [Benchmark(Baseline = true)]
    public void Adonet_Raw()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT \"Id\", \"Name\", \"Email\" FROM \"Users\" WHERE \"Id\" = @id";
        cmd.Parameters.AddWithValue("@id", 1);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {  }
    }

    [Benchmark]
    public void Dapper_Select()
    {
        var user = _connection.Query<User>("SELECT \"Id\", \"Name\", \"Email\" FROM \"Users\" WHERE \"Id\" = @id", new { id = 1 }).First();
    }

    [Benchmark]
    public async Task Drizzle_Select()
    {
        var user = await query;
    }
    
    [Benchmark]
    public async Task EfCore_Select()
    {
        var res = await _efContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == 1);
    }
}

class EntryPoint
{
    public static void Main()
    {
        BenchmarkRunner.Run<OrmBenchmark>();
    }
}
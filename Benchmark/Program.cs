using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dapper;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Dialect;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;
using Test.Shared;

[MemoryDiagnoser]
public class OrmBenchmark
{
    public NpgsqlConnection Connection;
    
    public DbClient<PgSqlSqlDialectImpl> Db;
    public static readonly UsersTable Users = new UsersTable();
    
    public AppDbContext EfContext;
    
    [GlobalSetup]
    public void Setup()
    {
        var connString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        
        var builder = new NpgsqlSlimDataSourceBuilder(connString);
        var dataSource = builder.Build();
        Connection = dataSource.OpenConnection(); 
        
        Db = new DbClient<PgSqlSqlDialectImpl>(Connection); 
        
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(Connection)
            .Options;
        EfContext = new AppDbContext(options);
        _ = EfContext.Users?.AsNoTracking()?.FirstOrDefault(u => u.Id == 1);
    }
    
    public User MapUser(NpgsqlDataReader reader)
    {
        var ordId = reader.GetOrdinal("Id");
        var ordGuid = reader.GetOrdinal("Guid");
        var ordName = reader.GetOrdinal("Name");
        var ordEmail = reader.GetOrdinal("Email");
        var ordAge = reader.GetOrdinal("Age");
        var ordSalary = reader.GetOrdinal("Salary");
        var ordRating = reader.GetOrdinal("Rating");
        var ordIsActive = reader.GetOrdinal("IsActive");
        var ordDepartmentId = reader.GetOrdinal("DepartmentId");
        var ordManagerId = reader.GetOrdinal("ManagerId");
        var ordRoleId = reader.GetOrdinal("RoleId");
        var ordCreatedAt = reader.GetOrdinal("CreatedAt");
        var ordUpdatedAt = reader.GetOrdinal("UpdatedAt");

        return new User
        {
            Id = reader.GetInt32(ordId),
            Guid = reader.GetGuid(ordGuid),
            Name = reader.GetString(ordName),
            Email = reader.GetString(ordEmail),
            Age = reader.GetInt32(ordAge),
            Salary = reader.GetDecimal(ordSalary),
            Rating = reader.GetDouble(ordRating),
            IsActive = reader.GetBoolean(ordIsActive),
            DepartmentId = reader.GetInt32(ordDepartmentId),
            ManagerId = reader.IsDBNull(ordManagerId) ? null : reader.GetInt32(ordManagerId),
            RoleId = reader.GetInt32(ordRoleId),
            CreatedAt = reader.GetDateTime(ordCreatedAt),
            UpdatedAt = reader.IsDBNull(ordUpdatedAt) ? null : reader.GetDateTime(ordUpdatedAt)
        };
    }
}

public class SelectOne: OrmBenchmark
{
    public static readonly string Sql = @"SELECT 
    ""Id"",
    ""Guid"",
    ""Name"",
    ""Email"",
    ""Age"",
    ""Salary"",
    ""Rating"",
    ""IsActive"",
    ""DepartmentId"",
    ""ManagerId"",
    ""RoleId"",
    ""CreatedAt"",
    ""UpdatedAt""
FROM ""Users""
WHERE ""Id"" = @id;";
    
    [Benchmark(Baseline = true)]
    public void Adonet_Raw_Select_One()
    {
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Parameters.AddWithValue("@id", 1);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var user = MapUser(reader);
        }
    }
    
    [Benchmark]
    public void Dapper_Select_One()
    {
        var user = Connection.Query<User>(Sql, new { id = 1 }).First();
    }

    [Benchmark]
    public async Task Drizzle_Select_One()
    {
        var user = await Db.Select(UsersTable.ModelAll)
            .From(Users)
            .Where(Eq(UsersTable.Id, 1));
    }
    
    [Benchmark]
    public async Task EfCore_Select_One()
    {
        var res = await EfContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == 1);
    }
}


public class SelectAll: OrmBenchmark
{
    public static readonly string Sql = @"SELECT 
    ""Id"",
    ""Guid"",
    ""Name"",
    ""Email"",
    ""Age"",
    ""Salary"",
    ""Rating"",
    ""IsActive"",
    ""DepartmentId"",
    ""ManagerId"",
    ""RoleId"",
    ""CreatedAt"",
    ""UpdatedAt""
FROM ""Users"";";
    
    [Benchmark(Baseline = true)]
    public void Adonet_Raw_Select_All()
    {
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = Sql;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var user = MapUser(reader);
        }
    }
    
    [Benchmark]
    public void Dapper_Select_All()
    {
        var user = Connection.Query<User>(Sql, new { id = 1 }).AsList();
    }

    [Benchmark]
    public async Task Drizzle_Select_All()
    {
        var user = await Db.Select(UsersTable.ModelAll)
            .From(Users);
    }
    
    [Benchmark]
    public async Task EfCore_Select_All()
    {
        var res = await EfContext.Users
            .AsNoTracking()
            .ToListAsync();
    }
}

class EntryPoint
{
    public static void Main()
    {
        BenchmarkRunner.Run<SelectOne>();
        BenchmarkRunner.Run<SelectAll>();
    }
}
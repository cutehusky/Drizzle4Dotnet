using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dapper;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Dialect;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedDemo;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

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
        // var ordId = reader.GetOrdinal("Id");
        // var ordGuid = reader.GetOrdinal("Guid");
        // var ordName = reader.GetOrdinal("Name");
        // var ordEmail = reader.GetOrdinal("Email");
        // var ordAge = reader.GetOrdinal("Age");
        // var ordSalary = reader.GetOrdinal("Salary");
        // var ordRating = reader.GetOrdinal("Rating");
        // var ordIsActive = reader.GetOrdinal("IsActive");
        // var ordDepartmentId = reader.GetOrdinal("DepartmentId");
        // var ordManagerId = reader.GetOrdinal("ManagerId");
        // var ordRoleId = reader.GetOrdinal("RoleId");
        // var ordCreatedAt = reader.GetOrdinal("CreatedAt");
        // var ordUpdatedAt = reader.GetOrdinal("UpdatedAt");

        // return new User
        // {
        //     Id = reader.GetInt32(ordId),
        //     Guid = reader.GetGuid(ordGuid),
        //     Name = reader.GetString(ordName),
        //     Email = reader.GetString(ordEmail),
        //     Age = reader.GetInt32(ordAge),
        //     Salary = reader.GetDecimal(ordSalary),
        //     Rating = reader.GetDouble(ordRating),
        //     IsActive = reader.GetBoolean(ordIsActive),
        //     DepartmentId = reader.GetInt32(ordDepartmentId),
        //     ManagerId = reader.IsDBNull(ordManagerId) ? null : reader.GetInt32(ordManagerId),
        //     RoleId = reader.GetInt32(ordRoleId),
        //     CreatedAt = reader.GetDateTime(ordCreatedAt),
        //     UpdatedAt = reader.IsDBNull(ordUpdatedAt) ? null : reader.GetDateTime(ordUpdatedAt)
        // };
        return new User
        {
            Id = reader.GetInt32(0),
            Guid = reader.GetGuid(1),
            Name = reader.GetString(2),
            Email = reader.GetString(3),
            Age = reader.GetInt32(4),
            Salary = reader.GetDecimal(5),
            Rating = reader.GetDouble(6),
            IsActive = reader.GetBoolean(7),
            DepartmentId = reader.GetInt32(8),
            ManagerId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
            RoleId = reader.GetInt32(10),
            CreatedAt = reader.GetDateTime(11),
            UpdatedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
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
    public async Task Adonet_Raw_Select_One()
    {
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Parameters.AddWithValue("@id", 1);
        await using var reader = await cmd.ExecuteReaderAsync();
        var users = new List<User>();
        while (await reader.ReadAsync())
        {
            var user = MapUser(reader);
            users.Add(user);
        }
    }
    
    [Benchmark]
    public async Task Dapper_Select_One()
    {
        var user = (await Connection.QueryAsync<User>(Sql, new { id = 1 })).First();
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
    public async Task Adonet_Raw_Select_All_Async()
    {
        await using var cmd = Connection.CreateCommand();
        cmd.CommandText = Sql;
        await using var reader = await cmd.ExecuteReaderAsync();
        var users = new List<User>();
        while (await reader.ReadAsync())
        {
            var user = MapUser(reader);
            users.Add(user);
        }
    }

    [Benchmark]
    public async Task Dapper_Select_All_Async()
    {
        var users = (await Connection.QueryAsync<User>(Sql)).AsList();
    }


    [Benchmark]
    public async Task Drizzle_Select_All()
    {
        var users = await Db.Select(UsersTable.ModelAll)
            .From(Users);
    }
    
    [Benchmark]
    public async Task EfCore_Select_All()
    {
        var users = await EfContext.Users
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
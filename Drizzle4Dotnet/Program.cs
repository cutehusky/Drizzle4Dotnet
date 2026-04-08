using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using MyNamespace;
using Npgsql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;


[DbSelect]
public partial class UserSelect1
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set;}
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set;}
    
    [MapWith(typeof(DepartmentsTable), DepartmentsTable.ColumnNames.Name)]
    public int DepartmentName { get; set;}
    
    [MapWith(typeof(ManagersTable), ManagersTable.ColumnNames.Name)]
    public int ManagerName { get;set; }
}


[DbSelect]
public partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get;set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set;}
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set;}
}


public class EntryPoint {
    public static async Task Main()
    {
        var connString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        
        var builder = new NpgsqlSlimDataSourceBuilder(connString);
        await using var dataSource = builder.Build();
        await using var conn = await dataSource.OpenConnectionAsync();

        var db = new DbClient<PgSqlSqlDialectImpl>(conn);
        var users = new UsersTable();
        var departments = new DepartmentsTable();
        var managers = new ManagersTable();

        var insertQuery = db.Insert(users)
            .Values(new UsersTable.InsertRecord
            {
                Id = 1,
                Name = "John Doe",
                Email = "test@email.com",
            }).Returning(UserSelect.Record);
        var (sql, parameters) = insertQuery.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }
        
        try {
            var insertResult = await insertQuery;
            var insertedRecord = insertResult[0];
            Console.WriteLine($"Inserted record: {insertedRecord}");
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine($"Insert error: {e.Message}");
        }

        var query = db
            .Select(UserSelect1.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(And(
                Eq(DepartmentsTable.Id, 1),
                Like(UsersTable.Email, "%@example.com")
                ));
        (sql, parameters) = query.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }

        try
        {
            var result = await query;
            var res = result[0];
            Console.WriteLine(res);
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }
        
        var query1 = db
            .Select(UserSelect.Mapping)
            .From(users)
            .Where(Eq(UsersTable.Id, 1));
        
        (sql, parameters) = query1.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }

        try
        {
            var result = await query1;
            var res = result[0];
            Console.WriteLine(res);
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }
        
        var query2 = db
            .Update(users)
            .Set(UsersTable.Name, "New Name")
            .Where(Eq(UsersTable.Id, 1))
            .Returning(UserSelect.Record);
        
        (sql, parameters) = query2.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }

        try
        {
            var result = await query2;
            var res = result[0];
            Console.WriteLine(res);
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }


        var query3 = db
            .Update(users)
            .Set(UsersTable.Name, "New Name 2")
            .Where(Eq(UsersTable.Id, 1));

        (sql, parameters) = query3.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }
        
        try
        {
            await query3;
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }
        
        var query4 = db.Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.Id, 1));
        (sql, parameters) = query4.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }
        
        
        var query5 = db
            .Update(users)
            .Set(UsersTable.Name, Concat(UsersTable.Name, " Jr."))
            .Where(Eq(UsersTable.Id, 1));

        (sql, parameters) = query5.Build();
        Console.WriteLine(sql);
        foreach (var queryParameter in parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }
        
        try
        {
            await query5;
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }
        
        var query6 = db
            .Update(users)
            .Set(new UsersTable.UpdateModel()
            {
                DepartmentId = 12
            })
            .Where(Eq(UsersTable.Id, 1));
        
        var query7 = db.Delete(users)
            .Where(Eq(UsersTable.Id, 1));
    }
}

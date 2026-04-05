using Drizzle_Like;
using Drizzle_Like.Query.Select;
using MyNamespace;
using Npgsql;
using static Drizzle_Like.Query.Shared.Operators.Operators;


[DbSelect]
public static partial class UserSelect1
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public static int Id { get; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public static string Email { get; }
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public static string Name { get; }
    
    [MapWith(typeof(DepartmentsTable), DepartmentsTable.ColumnNames.Name)]
    public static int DepartmentName { get; }
    
    [MapWith(typeof(ManagersTable), ManagersTable.ColumnNames.Name)]
    public static int ManagerName { get; }
}


[DbSelect]
public static partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public static int Id { get; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public static string Email { get; }
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public static string Name { get; }
}


public class EntryPoint {
    public static async Task Main()
    {
        var connString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        
        var builder = new NpgsqlSlimDataSourceBuilder(connString);
        await using var dataSource = builder.Build();
        await using var conn = await dataSource.OpenConnectionAsync();

        var db = new DbClient(conn);
        var users = new UsersTable();
        var departments = new DepartmentsTable();
        var managers = new ManagersTable();

        var query = db
            .Select(UserSelect1.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(And(
                Eq(DepartmentsTable.Id, 1),
                Like(UsersTable.Email, "%@example.com")
                ));

        Console.WriteLine(query.Sql);
        foreach (var queryParameter in query.Parameters)
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
            .Select(UserSelect.Model)
            .From(users)
            .Where(Eq(UsersTable.Id, 1));
        
        Console.WriteLine(query1.Sql);
        foreach (var queryParameter in query1.Parameters)
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

        Console.WriteLine(query2.Sql);
        foreach (var queryParameter in query2.Parameters)
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

        Console.WriteLine(query3.Sql);
        foreach (var queryParameter in query3.Parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }

        try
        {
            var result = await query3;
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e.Message);
        }
        
        var query4 = db.Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.Id, 1));
        Console.WriteLine(query3.Sql);
        foreach (var queryParameter in query3.Parameters)
        {
            Console.WriteLine($"{queryParameter.Key} = {queryParameter.Value}");
        }
        
    }
}

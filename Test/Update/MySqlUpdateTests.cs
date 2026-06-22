
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.MySql;
using SharedDemo.MySql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Update;

[TestFixture]
public class MySqlUpdateTests
{
    private MySqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private DepartmentsTable departments = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();
        users = new UsersTable();
        departments = new DepartmentsTable();
    }

    private void Print(string title, string sql, Dictionary<string, object?> parameters)
    {
        TestContext.Out.WriteLine("===== " + title + " =====");
        TestContext.Out.WriteLine(sql);
        foreach (var p in parameters)
        {
            TestContext.Out.WriteLine(p.Key + ": " + p.Value);
        }
        TestContext.Out.WriteLine("");
    }

    [Test]
    public void Update_Basic()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, "Updated Name")
            .Set(UsersTable.Email, "updated@example.com")
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE basic", sql, parameters);
    }

    [Test]
    public void Update_MultipleConditions()
    {
        var query = _db.Update(users)
            .Set(UsersTable.IsActive, false)
            .Where(
                Eq(UsersTable.DepartmentId, 2),
                Eq(UsersTable.RoleId, 3)
            );

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with multiple WHERE", sql, parameters);
    }

    [Test]
    public void Update_WithExpressionValue()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Salary, Add(UsersTable.Salary, 5000m))
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with expression value", sql, parameters);
    }

    [Test]
    public void Update_WithJoin()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, Sql.Raw("CONCAT(`Users`.`Name`, ' - Updated')"))
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(Eq(DepartmentsTable.Name, "Engineering"));

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with JOIN", sql, parameters);
    }

    [Test]
    public void Update_WithLeftJoin()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, Sql.Raw("'Archived'"))
            .LeftJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(IsNull(DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with LEFT JOIN", sql, parameters);
    }

    [Test]
    public void Update_WithCrossJoin()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, Sql.Raw("'Default'"))
            .CrossJoin(departments);

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with CROSS JOIN", sql, parameters);
    }

    [Test]
    public void Update_WithOrderByAndLimit()
    {
        var query = _db.Update(users)
            .Set(UsersTable.IsActive, false)
            .Where(Eq(UsersTable.IsActive, true))
            .OrderBy(UsersTable.Id)
            .Limit(10);

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with ORDER BY and LIMIT", sql, parameters);
    }

    [Test]
    public void Update_WithRightJoin()
    {
        var query = _db.Update(users)
            .Set(UsersTable.IsActive, true)
            .RightJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL UPDATE with RIGHT JOIN", sql, parameters);
    }
}

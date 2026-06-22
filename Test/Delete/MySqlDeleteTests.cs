using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.MySql;
using SharedDemo.MySql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Delete;

[TestFixture]
public class MySqlDeleteTests
{
    private MySqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private DepartmentsTable departments = null!;
    private UserProjectsTable userProjects = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();
        users = new UsersTable();
        departments = new DepartmentsTable();
        userProjects = new UserProjectsTable();
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
    public void Delete_Basic()
    {
        var query = _db.Delete(users)
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE basic", sql, parameters);
    }

    [Test]
    public void Delete_All()
    {
        var query = _db.Delete(users);

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE all", sql, parameters);
    }

    [Test]
    public void Delete_MultipleConditions()
    {
        var query = _db.Delete(users)
            .Where(
                Eq(UsersTable.IsActive, false),
                Eq(UsersTable.DepartmentId, 5)
            );

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with multiple WHERE", sql, parameters);
    }

    [Test]
    public void Delete_WithJoin()
    {
        var query = ((MySqlDeleteQuery<UsersTable>)_db.Delete(users))
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(Eq(DepartmentsTable.Name, "Archived"));

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with INNER JOIN", sql, parameters);
    }

    [Test]
    public void Delete_WithLeftJoin()
    {
        var query = ((MySqlDeleteQuery<UsersTable>)_db.Delete(users))
            .LeftJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(IsNull(DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with LEFT JOIN", sql, parameters);
    }

    [Test]
    public void Delete_WithCrossJoin()
    {
        var query = ((MySqlDeleteQuery<UsersTable>)_db.Delete(users))
            .CrossJoin(departments);

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with CROSS JOIN", sql, parameters);
    }

    [Test]
    public void Delete_WithOrderByAndLimit()
    {
        var query = _db.Delete(users)
            .OrderBy(UsersTable.Id)
            .Limit(100)
            .Where(Eq(UsersTable.IsActive, false));

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with ORDER BY and LIMIT", sql, parameters);
    }

    [Test]
    public void Delete_WithRightJoin()
    {
        var query = ((MySqlDeleteQuery<UsersTable>)_db.Delete(users))
            .RightJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with RIGHT JOIN", sql, parameters);
    }

    [Test]
    public void Delete_WithComplexWhere()
    {
        var query = _db.Delete(users)
            .Where(
                And(
                    Lt(UsersTable.Age, 18),
                    Eq(UsersTable.IsActive, false)
                )
            );

        var (sql, parameters) = query.Build();
        Print("MySQL DELETE with complex WHERE", sql, parameters);
    }
}

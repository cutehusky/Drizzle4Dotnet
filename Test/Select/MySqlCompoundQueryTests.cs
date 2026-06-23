using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.MySql;
using SharedDemo.MySql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Select;

[TestFixture]
public class MySqlCompoundQueryTests
{
    private MySqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private UserProjectsTable userProjects = null!;
    private DepartmentsTable departments = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();
        users = new UsersTable();
        userProjects = new UserProjectsTable();
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
    public void Union_Basic()
    {
        var left = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var right = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false));

        var query = left.Union(right);

        var (sql, parameters) = query.Build();
        Print("MySQL UNION", sql, parameters);
    }

    [Test]
    public void UnionAll_Basic()
    {
        var left = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Gt(UsersTable.Age, 30));

        var right = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Ltq(UsersTable.Age, 30));

        var query = left.UnionAll(right);

        var (sql, parameters) = query.Build();
        Print("MySQL UNION ALL", sql, parameters);
    }

    [Test]
    public void Intersect_Basic()
    {
        var query1 = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Gt(UsersTable.Age, 25));

        var query2 = _db
            .Select(UserProjectsTable.UserId)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.IsActive, true));

        var result = query1.Intersect(query2);

        var (sql, parameters) = result.Build();
        Print("MySQL INTERSECT", sql, parameters);
    }

    [Test]
    public void Except_Basic()
    {
        var allUsers = _db
            .Select(UsersTable.Id)
            .From(users);

        var projectMembers = _db
            .Select(UserProjectsTable.UserId)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.IsActive, true));

        var result = allUsers.Except(projectMembers);

        var (sql, parameters) = result.Build();
        Print("MySQL EXCEPT", sql, parameters);
    }

    [Test]
    public void UnionAll_WithSubquery()
    {
        var left = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.DepartmentId, 1));

        var right = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.DepartmentId, 2));

        var unionResult = left.UnionAll(right);

        var (sql, parameters) = unionResult.Build();
        Print("MySQL UNION ALL - department merge", sql, parameters);
    }

    // =========================================================================
    // MySQL WITH RECURSIVE CTE
    // =========================================================================

    [Test]
    public void Select_WithRecursiveCte()
    {
        var cte = _db
            .Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
            .From(departments)
            .Where(Eq(DepartmentsTable.ParentDepartmentId, Sql.Value<long?>(null)))
            .UnionAll(
                _db.Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
                    .From(departments)
                    .Where(Gt(DepartmentsTable.Id, 0))
            )
            .AsRecursiveCte("dept_tree");

        var query = _db
            .Select(cte.Field<long>("Id"), cte.Field<string>("Name"))
            .WithRecursive(cte)
            .From(cte)
            .OrderBy(cte.Field<long>("Id"));

        var (sql, parameters) = query.Build();
        Print("MySQL WITH RECURSIVE CTE", sql, parameters);
    }

    [Test]
    public void Select_WithCte()
    {
        var cte = _db
            .Select(DepartmentsTable.Id, DepartmentsTable.Name)
            .From(departments)
            .Where(Eq(DepartmentsTable.IsActive, true))
            .AsSubQuery("active_depts", (from) => new
            {
                Id = from.Field<long>("Id"),
                Name = from.Field<string>("Name")
            }).AsCte();

        var query = _db
            .Select(cte.Field<long>("Id"), cte.Field<string>("Name"))
            .With(cte)
            .From(cte);

        var (sql, parameters) = query.Build();
        Print("MySQL WITH CTE", sql, parameters);
    }
}

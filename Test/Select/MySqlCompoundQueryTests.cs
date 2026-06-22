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

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();
        users = new UsersTable();
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
}

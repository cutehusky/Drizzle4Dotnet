using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql;
using SharedDemo.PgSql;
using static Drizzle4Dotnet.Core.Operators.Operators;
using Drizzle4Dotnet.Core.Query;

namespace Test.Update;

[TestFixture]
public class PgSqlUpdateTests
{
    private PgSqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private DepartmentsTable departments = null!;

    [SetUp]
    public void Setup()
    {
        _db = new PgSqlQueryBuilder();
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
        Print("PgSQL UPDATE basic", sql, parameters);
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
        Print("PgSQL UPDATE with multiple WHERE", sql, parameters);
    }

    [Test]
    public void Update_WithExpressionValue()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Salary, Add(UsersTable.Salary, 5000m))
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with expression value", sql, parameters);
    }

    [Test]
    public void Update_WithFrom()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, DepartmentsTable.Name)
            .From(departments)
            .Where(And(
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
                Eq(DepartmentsTable.Name, "Engineering")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with FROM", sql, parameters);
    }

    [Test]
    public void Update_WithMultipleFromTables()
    {
        var query = _db.Update(users)
            .Set(UsersTable.IsActive, true)
            .From(departments);

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with multiple FROM", sql, parameters);
    }

    [Test]
    public void Update_WithSetRecord()
    {
        var query = _db.Update(users)
            .Set(new UsersTable.UpdateRecord
            {
                Name = "New Name",
                Email = "new@example.com",
                UpdatedAt = DateTime.UtcNow
            })
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with record Set", sql, parameters);
    }

    [Test]
    public void Update_WithReturning()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, "Updated Name")
            .Where(Eq(UsersTable.Id, 1))
            .Returning(UsersTable.ModelAll);

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE RETURNING", sql, parameters);
    }

    [Test]
    public void Update_WithCte()
    {
        var cte = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false))
            .AsSubQuery("inactive_users", (from) => new
            {
                Id = from.Field<long>("Id")
            }).AsCte();

        var query = _db.Update(users)
            .With(cte)
            .Set(UsersTable.IsActive, true)
            .Where(In(UsersTable.Id, Sql.Raw<long>("SELECT Id FROM inactive_users")));

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with CTE", sql, parameters);
    }

    // =========================================================================
    // CTE + RETURNING combination tests
    // =========================================================================

    [Test]
    public void Update_WithCteAndReturning()
    {
        var cte = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false))
            .AsSubQuery("inactive_users", (from) => new
            {
                Id = from.Field<long>("Id")
            }).AsCte();

        var query = _db.Update(users)
            .With(cte)
            .Set(UsersTable.IsActive, true)
            .Where(In(UsersTable.Id, Sql.Raw<long>("SELECT Id FROM inactive_users")))
            .Returning(UsersTable.ModelAll);

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE with CTE + RETURNING", sql, parameters);
    }

    [Test]
    public void Update_WithFromAndReturning()
    {
        var query = _db.Update(users)
            .Set(UsersTable.Name, DepartmentsTable.Name)
            .From(departments)
            .Where(And(
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
                Eq(DepartmentsTable.Name, "Engineering")
            ))
            .Returning(UsersTable.Id, UsersTable.Name, UsersTable.Email);

        var (sql, parameters) = query.Build();
        Print("PgSQL UPDATE FROM + RETURNING", sql, parameters);
    }
}

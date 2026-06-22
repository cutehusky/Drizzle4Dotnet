using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.PgSql;
using SharedDemo.PgSql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Delete;

[TestFixture]
public class PgSqlDeleteTests
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
    public void Delete_Basic()
    {
        var query = _db.Delete(users)
            .Where(Eq(UsersTable.Id, 1));

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE basic", sql, parameters);
    }

    [Test]
    public void Delete_All()
    {
        var query = _db.Delete(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE all", sql, parameters);
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
        Print("PgSQL DELETE with multiple WHERE", sql, parameters);
    }

    [Test]
    public void Delete_WithUsing()
    {
        var query = ((PgDeleteQuery<UsersTable>)_db.Delete(users))
            .Using(departments)
            .Where(And(
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
                Eq(DepartmentsTable.Name, "Archived")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE USING", sql, parameters);
    }

    [Test]
    public void Delete_WithMultipleUsing()
    {
        var query = ((PgDeleteQuery<UsersTable>)_db.Delete(users))
            .Using(departments);

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE multiple USING", sql, parameters);
    }

    [Test]
    public void Delete_WithComplexWhere()
    {
        var query = _db.Delete(users)
            .Where(
                Or(
                    Lt(UsersTable.Age, 18),
                    Gt(UsersTable.Age, 65)
                ),
                Eq(UsersTable.IsActive, false)
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE with complex WHERE", sql, parameters);
    }

    [Test]
    public void Delete_WithReturning()
    {
        var query = _db.Delete(users)
            .Where(Eq(UsersTable.Id, 1))
            .Returning(UsersTable.ModelAll);

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE RETURNING", sql, parameters);
    }

    [Test]
    public void Delete_WithCte()
    {
        var cte = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false))
            .AsSubQuery("inactive_users", (from) => new
            {
                Id = from.Field<long>("Id")
            }).AsCte();

        var query = _db.Delete(users)
            .With(cte)
            .Where(In(UsersTable.Id, Sql.Raw<long>("SELECT Id FROM inactive_users")));

        var (sql, parameters) = query.Build();
        Print("PgSQL DELETE with CTE", sql, parameters);
    }
}

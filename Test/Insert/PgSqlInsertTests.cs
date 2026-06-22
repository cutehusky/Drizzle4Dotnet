using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.PgSql;
using SharedDemo.PgSql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Insert;

[TestFixture]
public class PgSqlInsertTests
{
    private PgSqlQueryBuilder _db = null!;
    private UsersTable users = null!;

    [SetUp]
    public void Setup()
    {
        _db = new PgSqlQueryBuilder();
        users = new UsersTable();
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
    public void Insert_Basic()
    {
        var query = _db.Insert(users).Value(new UsersTable.InsertRecord
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            DepartmentId = 1,
            RoleId = 1
        });

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT basic", sql, parameters);
    }

    [Test]
    public void Insert_MultipleValues()
    {
        var query = _db.Insert(users).Values(
            new UsersTable.InsertRecord { Name = "Alice", Email = "alice@example.com", Age = 25, IsActive = true, DepartmentId = 1, RoleId = 1 },
            new UsersTable.InsertRecord { Name = "Bob", Email = "bob@example.com", Age = 28, IsActive = true, DepartmentId = 2, RoleId = 2 }
        );

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT multiple rows", sql, parameters);
    }

    [Test]
    public void Insert_WithDictionary()
    {
        var query = _db.Insert(users).Value(new Dictionary<IColumnOfTable<UsersTable>, object?>
        {
            { UsersTable.Name, "John" },
            { UsersTable.Email, "john@example.com" },
            { UsersTable.Age, 30 }
        });

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT with Dictionary", sql, parameters);
    }

    [Test]
    public void Insert_OnConflictDoNothing()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnConflictDoNothing("(Email)");

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT ON CONFLICT DO NOTHING", sql, parameters);
    }

    [Test]
    public void Insert_OnConflictDoUpdate()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnConflictDoUpdate("(Email)",
                ("Name", "EXCLUDED"),
                ("Age", "EXCLUDED")
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT ON CONFLICT DO UPDATE", sql, parameters);
    }

    [Test]
    public void Insert_OnConflictOnConstraint()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord{ Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnConflictOnConstraint("users_email_key");

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT ON CONFLICT ON CONSTRAINT", sql, parameters);
    }

    [Test]
    public void Insert_DefaultValues()
    {
        var query = _db.Insert(users)
            .DefaultValues();

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT DEFAULT VALUES", sql, parameters);
    }

    [Test]
    public void Insert_SelectFrom()
    {
        var selectQuery = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db.Insert(users).From(selectQuery);

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT SELECT", sql, parameters);
    }

    [Test]
    public void Insert_WithReturning()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .Returning(UsersTable.ModelAll);

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT RETURNING", sql, parameters);
    }

    [Test]
    public void Insert_WithCte()
    {
        var cte = _db
            .Select(UsersTable.Name, UsersTable.Email)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_users", (from) => new
            {
                Name = from.Field<string>("Name"),
                Email = from.Field<string>("Email")
            }).AsCte();

        var query = _db.Insert(users)
            .With(cte)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 });

        var (sql, parameters) = query.Build();
        Print("PgSQL INSERT with CTE", sql, parameters);
    }
}

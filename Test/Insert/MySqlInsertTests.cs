using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.MySql;
using SharedDemo.MySql;
using static Drizzle4Dotnet.Core.Operators.Operators;
using static Drizzle4Dotnet.Core.Operators.Functions;

namespace Test.Insert;

[TestFixture]
public class MySqlInsertTests
{
    private MySqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private ProjectsTable projects = null!;
    private UserProjectsTable userProjects = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();
        users = new UsersTable();
        projects = new ProjectsTable();
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
    public void Insert_Basic()
    {
        var query = _db.Insert(users).Value(new UsersTable.InsertRecord()
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            DepartmentId = 1,
            RoleId = 1
        });

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT basic", sql, parameters);
    }

    [Test]
    public void Insert_MultipleValues()
    {
        var query = _db.Insert(users).Values(
            new UsersTable.InsertRecord { Name = "Alice", Email = "alice@example.com", Age = 25, IsActive = true, DepartmentId = 1, RoleId = 1 },
            new UsersTable.InsertRecord { Name = "Bob", Email = "bob@example.com", Age = 28, IsActive = true, DepartmentId = 2, RoleId = 2 },
            new UsersTable.InsertRecord { Name = "Charlie", Email = "charlie@example.com", Age = 35, IsActive = false, DepartmentId = 1, RoleId = 3 }
        );

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT multiple rows", sql, parameters);
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
        Print("MySQL INSERT with Dictionary", sql, parameters);
    }

    [Test]
    public void Insert_Ignore()
    {
        var query = _db.Insert(users)
            .Ignore()
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 });

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT IGNORE", sql, parameters);
    }

    [Test]
    public void Insert_OnDuplicateKeyUpdate()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnDuplicateKeyUpdateValues(UsersTable.Name, UsersTable.Email);

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT ON DUPLICATE KEY UPDATE", sql, parameters);
    }

    [Test]
    public void Insert_OnDuplicateKeyUpdateAll()
    {
        var query = _db.Insert(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnDuplicateKeyUpdateValues(UsersTable.Name, UsersTable.Email, UsersTable.Age, UsersTable.IsActive, UsersTable.DepartmentId, UsersTable.RoleId);

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT ON DUPLICATE KEY UPDATE ALL", sql, parameters);
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
        Print("MySQL INSERT SELECT", sql, parameters);
    }

    [Test]
    public void Replace_Basic()
    {
        var query = _db.Replace(users)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 });

        var (sql, parameters) = query.Build();
        Print("MySQL REPLACE INTO", sql, parameters);
    }

    [Test]
    public void Replace_MultipleRows()
    {
        var query = _db.Replace(users)
            .Values(
                new UsersTable.InsertRecord { Name = "Alice", Email = "alice@e.com", Age = 25, IsActive = true, DepartmentId = 1, RoleId = 1 },
                new UsersTable.InsertRecord { Name = "Bob", Email = "bob@e.com", Age = 30, IsActive = true, DepartmentId = 2, RoleId = 2 }
            );

        var (sql, parameters) = query.Build();
        Print("MySQL REPLACE multiple rows", sql, parameters);
    }

    // =========================================================================
    // MySQL INSERT with CTE
    // =========================================================================

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
        Print("MySQL INSERT with CTE", sql, parameters);
    }

    [Test]
    public void Insert_WithCteAndOnDuplicateKeyUpdate()
    {
        var cte = _db
            .Select(UsersTable.Email)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_emails", (from) => new
            {
                Email = from.Field<string>("Email")
            }).AsCte();

        var query = _db.Insert(users)
            .With(cte)
            .Value(new UsersTable.InsertRecord { Name = "John", Email = "john@example.com", Age = 30, IsActive = true, DepartmentId = 1, RoleId = 1 })
            .OnDuplicateKeyUpdateValues(UsersTable.Name, UsersTable.Email);

        var (sql, parameters) = query.Build();
        Print("MySQL INSERT with CTE + ON DUPLICATE KEY UPDATE", sql, parameters);
    }
}

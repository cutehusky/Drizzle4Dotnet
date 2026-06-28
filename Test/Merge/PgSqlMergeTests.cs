using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql;
using SharedDemo.PgSql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Merge;

[TestFixture]
public class PgSqlMergeTests
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
    public void Merge_BasicUpsert()
    {
        // Basic MERGE with WHEN MATCHED UPDATE and WHEN NOT MATCHED INSERT
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\", 'New User' AS \"Name\", 'new@example.com' AS \"Email\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! },
                { "Email", null! }
            })
            .WhenNotMatchedThenInsert(
                new List<string> { "Id", "Name", "Email" },
                new List<object?> { null!, null!, null! }
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE basic upsert", sql, parameters);
        
        Assert.That(sql, Does.Contain("MERGE INTO"));
        Assert.That(sql, Does.Contain("USING"));
        Assert.That(sql, Does.Contain("ON ("));
        Assert.That(sql, Does.Contain("WHEN MATCHED THEN UPDATE SET"));
        Assert.That(sql, Does.Contain("WHEN NOT MATCHED THEN INSERT"));
    }

    [Test]
    public void Merge_WithUpdateOnly()
    {
        // MERGE with only WHEN MATCHED UPDATE (no insert)
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\", 'Updated Name' AS \"Name\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! }
            });

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE update only", sql, parameters);
        
        Assert.That(sql, Does.Contain("WHEN MATCHED THEN UPDATE SET"));
        Assert.That(sql, Does.Not.Contain("WHEN NOT MATCHED"));
    }

    [Test]
    public void Merge_WithInsertOnly()
    {
        // MERGE with only WHEN NOT MATCHED INSERT
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\", 'New User' AS \"Name\", 'new@example.com' AS \"Email\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenNotMatchedThenInsert(
                new List<string> { "Id", "Name", "Email" },
                new List<object?> { null!, null!, null! }
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE insert only", sql, parameters);
        
        Assert.That(sql, Does.Contain("WHEN NOT MATCHED THEN INSERT"));
        Assert.That(sql, Does.Not.Contain("WHEN MATCHED"));
    }

    [Test]
    public void Merge_WithDeleteBySource()
    {
        // MERGE with WHEN NOT MATCHED BY SOURCE THEN DELETE
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\", 'Name' AS \"Name\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! }
            })
            .WhenNotMatchedThenInsert(
                new List<string> { "Id", "Name" },
                new List<object?> { null!, null! }
            )
            .WhenNotMatchedBySourceThenDelete();

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE with delete by source", sql, parameters);
        
        Assert.That(sql, Does.Contain("WHEN NOT MATCHED BY SOURCE THEN DELETE"));
    }

    [Test]
    public void Merge_WithReturning()
    {
        // MERGE with RETURNING clause (PostgreSQL-specific)
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\", 'New User' AS \"Name\", 'new@example.com' AS \"Email\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! },
                { "Email", null! }
            })
            .WhenNotMatchedThenInsert(
                new List<string> { "Id", "Name", "Email" },
                new List<object?> { null!, null!, null! }
            )
            .Returning("Id", "Name", "Email");

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE with RETURNING", sql, parameters);
        
        Assert.That(sql, Does.Contain("RETURNING"));
        Assert.That(sql, Does.Contain("\"Id\""));
        Assert.That(sql, Does.Contain("\"Name\""));
    }

    [Test]
    public void Merge_WithCte()
    {
        // MERGE with CTE
        var cte = _db
            .Select(UsersTable.Id, UsersTable.Name, UsersTable.Email)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false))
            .AsSubQuery("source_users", (from) => new
            {
                Id = from.Field<long>("Id"),
                Name = from.Field<string>("Name"),
                Email = from.Field<string>("Email")
            }).AsCte();

        var query = _db.Merge(users)
            .With(cte)
            .Using(Sql.Raw("SELECT \"Id\", \"Name\", \"Email\" FROM \"source_users\""))
            .On(Sql.Raw("target.\"Id\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! },
                { "Email", null! }
            })
            .WhenNotMatchedThenInsert(
                new List<string> { "Id", "Name", "Email" },
                new List<object?> { null!, null!, null! }
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE with CTE", sql, parameters);
        
        Assert.That(sql, Does.StartWith("WITH"));
        Assert.That(sql, Does.Contain("MERGE INTO"));
    }

    [Test]
    public void Merge_WithJoin()
    {
        // MERGE with JOIN on source
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT \"Id\", \"Name\" FROM \"Departments\""))
            .InnerJoin(departments, Sql.Raw("source.\"Id\" = \"departments\".\"Id\""))
            .On(Sql.Raw("target.\"DepartmentId\" = source.\"Id\""))
            .WhenMatchedThenUpdate(new Dictionary<string, object?>
            {
                { "Name", null! }
            });

        var (sql, parameters) = query.Build();
        Print("PgSQL MERGE with JOIN", sql, parameters);
        
        Assert.That(sql, Does.Contain("INNER JOIN"));
    }

    [Test]
    public void Merge_WhenSourceNotMatched_ThrowsWithoutSource()
    {
        // MERGE should throw if Using() is not called
        var query = _db.Merge(users);
        
        Assert.Throws<InvalidOperationException>(() => query.Build(), 
            "MERGE requires a source. Call Using().");
    }

    [Test]
    public void Merge_WhenSourceNotMatched_ThrowsWithoutOn()
    {
        // MERGE should throw if On() is not called
        var query = _db.Merge(users)
            .Using(Sql.Raw("SELECT 1 AS \"Id\""));

        Assert.Throws<InvalidOperationException>(() => query.Build(), 
            "MERGE requires an ON condition. Call On().");
    }
}

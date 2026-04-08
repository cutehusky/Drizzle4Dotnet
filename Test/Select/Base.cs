using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Dialect;
using Test.Shared;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

namespace Test.Select;

[TestFixture]
public class SelectQueryPgTests
{
    private QueryBuilder<PgSqlSqlDialectImpl> _db;

    private Shared.UsersTable users;
    private Shared.DepartmentsTable departments;
    private Shared.RolesTable roles;
    private Shared.ManagersTable managers;
    private Shared.ProjectsTable projects;
    private Shared.UserProjectsTable userProjects;

    [SetUp]
    public void Setup()
    {
        _db = new QueryBuilder<PgSqlSqlDialectImpl>();

        users = new Shared.UsersTable();
        departments = new Shared.DepartmentsTable();
        roles = new Shared.RolesTable();
        managers = new Shared.ManagersTable();
        projects = new Shared.ProjectsTable();
        userProjects = new Shared.UserProjectsTable();
    }
    
    private void Print(string title, string sql, Dictionary<string, object> parameters)
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
    public void Select_Basic()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();

        Print("Basic SELECT", sql, parameters);
    }

    [Test]
    public void Select_WithWhere()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .Where(And(
                Eq(Shared.UsersTable.Id, 1),
                Like(Shared.UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();

        Print("SELECT with WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithJoin()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(Shared.UsersTable.DepartmentId, Shared.DepartmentsTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELECT with JOIN", sql, parameters);
    }

    [Test]
    public void Select_WithMultipleJoins()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(Shared.UsersTable.DepartmentId, Shared.DepartmentsTable.Id))
            .InnerJoin(roles,
                Eq(Shared.UsersTable.RoleId, Shared.RolesTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELECT with MULTIPLE JOINS", sql, parameters);
    }

    [Test]
    public void Select_SelfJoin_Alias()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .InnerJoin(managers,
                Eq(Shared.UsersTable.ManagerId, Shared.ManagersTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELF JOIN (Alias)", sql, parameters);
    }

    [Test]
    public void Select_ManyToMany()
    {
        var query = _db
            .Select(Shared.ProjectSelect.Record)
            .From(userProjects)
            .InnerJoin(projects,
                Eq(Shared.UserProjectsTable.ProjectId, Shared.ProjectsTable.Id))
            .InnerJoin(users,
                Eq(Shared.UserProjectsTable.UserId, Shared.UsersTable.Id));

        var (sql, parameters) = query.Build();

        Print("MANY-TO-MANY JOIN", sql, parameters);
    }

    [Test]
    public void Select_ComplexWhere()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .Where(
                Eq(Shared.UsersTable.IsActive, true),
                Eq(Shared.UsersTable.Age, 30),
                Like(Shared.UsersTable.Email, "%@company.com")
            );

        var (sql, parameters) = query.Build();

        Print("COMPLEX WHERE", sql, parameters);
    }

    [Test]
    public void Select_NullCheck()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .Where(IsNull(Shared.UsersTable.ManagerId));

        var (sql, parameters) = query.Build();

        Print("NULL CHECK", sql, parameters);
    }

    [Test]
    public void Select_FullComplex()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(Shared.UsersTable.DepartmentId, Shared.DepartmentsTable.Id))
            .InnerJoin(managers,
                Eq(Shared.UsersTable.ManagerId, Shared.ManagersTable.Id))
            .Where(And(
                Eq(Shared.DepartmentsTable.Id, 1),
                Like(Shared.UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();

        Print("FULL COMPLEX QUERY", sql, parameters);
    }
    
    [Test]
    public void Select_WithOrderBy()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .OrderBy(Shared.UsersTable.Name)
            .OrderBy(Shared.UsersTable.Age, false);

        var (sql, parameters) = query.Build();

        Print("SELECT with ORDER BY", sql, parameters);
    }

    [Test]
    public void Select_WithLimitOffset()
    {
        var query = _db
            .Select(global::UserSelect.Record)
            .From(users)
            .OrderBy(Shared.UsersTable.Id)
            .Limit(10)
            .Offset(20);

        var (sql, parameters) = query.Build();

        Print("SELECT with LIMIT/OFFSET", sql, parameters);
    }

    // [Test]
    // public void Select_WithGroupBy()
    // {
    //     var query = _db
    //         .Select(
    //             DepartmentsTable.Id,
    //             DepartmentsTable.Name,
    //             Count(UsersTable.Id).As("UserCount")
    //         )
    //         .From(users)
    //         .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    //         .GroupBy(DepartmentsTable.Id, DepartmentsTable.Name)
    //         .Having(Gt(Count(UsersTable.Id), 5));
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with GROUP BY and HAVING", sql, parameters);
    // }

    [Test]
    public void Select_WithDistinct()
    {
        var query = _db
            .SelectDistinct(global::UserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();

        Print("SELECT DISTINCT", sql, parameters);
    }

    [Test]
    public void Select_WithSubquery()
    {
        var subQuery = _db
            .Select(Shared.UserSelect.Mapping)
            .From(users)
            .Where(Eq(Shared.UsersTable.IsActive, true));
    
        var query = _db
            .Select(Shared.UserSelect.Record)
            .From(users)
            .Where(In(Shared.UsersTable.Id, subQuery));
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with SUBQUERY", sql, parameters);
    }

    [Test]
    public void Select_WithComplexJoinsAndWhere()
    {
        var query = _db
            .Select(Shared.UserWithRelationsSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(Shared.UsersTable.DepartmentId, Shared.DepartmentsTable.Id))
            .InnerJoin(roles, Eq(Shared.UsersTable.RoleId, Shared.RolesTable.Id))
            .LeftJoin(managers, Eq(Shared.UsersTable.ManagerId, Shared.ManagersTable.Id))
            .Where(
                Eq(Shared.UsersTable.IsActive, true),
                Gt(Shared.UsersTable.Age, 25),
                Like(Shared.UsersTable.Email, "%@company.com")
            )
            .OrderBy(Shared.UsersTable.Name)
            .Limit(50);

        var (sql, parameters) = query.Build();

        Print("COMPLEX JOIN + WHERE + ORDER + LIMIT", sql, parameters);
    }

    // [Test]
    // public void Select_WithCaseExpression()
    // {
    //     var query = _db
    //         .Select(
    //             UsersTable.Id,
    //             Case()
    //                 .When(Eq(UsersTable.IsActive, true), "Active")
    //                 .Else("Inactive")
    //                 .As("Status")
    //         )
    //         .From(users);
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with CASE expression", sql, parameters);
    // }
    
    // [Test]
    // public void Select_WithAggregateFunctions()
    // {
    //     var query = _db
    //         .Select(
    //             Count(UsersTable.Id).As("TotalUsers"),
    //             Avg(UsersTable.Age).As("AverageAge"),
    //             Max(UsersTable.Salary).As("MaxSalary"),
    //             Min(UsersTable.Salary).As("MinSalary")
    //         )
    //         .From(users);
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with AGGREGATE functions", sql, parameters);
    // }
    
    [Test]
    public void Select_WithExists()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 1));

        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(Exists(subQuery));

        var (sql, parameters) = query.Build();

        Print("SELECT with EXISTS", sql, parameters);
    }

    [Test]
    public void Select_WithInSubquery()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 2));

        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();

        Print("SELECT with IN SUBQUERY", sql, parameters);
    }

    // [Test]
    // public void Select_FromSubquery_AsDerivedTable()
    // {
    //     var subQuery = _db
    //         .Select(UserSelect.Id, UserSelect.Email)
    //         .From(users)
    //         .Where(Eq(UsersTable.IsActive, true));
    //
    //     var derived = subQuery.As("active_users");
    //
    //     var query = _db
    //         .Select(derived.Column("Id"), derived.Column("Email"))
    //         .From(derived);
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT FROM SUBQUERY (Derived Table)", sql, parameters);
    // }

    // [Test]
    // public void Select_SubqueryInSelectList()
    // {
    //     var subQuery = _db
    //         .Select(Count(UserProjectsTable.ProjectId))
    //         .From(userProjects)
    //         .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));
    //
    //     var query = _db
    //         .Select(
    //             UsersTable.Id,
    //             UsersTable.Name,
    //             subQuery.As("ProjectCount")
    //         )
    //         .From(users);
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with SUBQUERY in SELECT list", sql, parameters);
    // }

    [Test]
    public void Select_ComplexExistsAndJoins()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));

        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(And(
                Exists(subQuery),
                Eq(DepartmentsTable.Name, "Engineering")
            ))
            .OrderBy(UsersTable.Name);

        var (sql, parameters) = query.Build();

        Print("COMPLEX SELECT with EXISTS and JOIN", sql, parameters);
    }

    [Test]
    public void Select_WithNestedSubqueries()
    {
        var innerSub = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 3));

        var outerSub = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(In(UsersTable.Id, innerSub));

        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, outerSub));

        var (sql, parameters) = query.Build();

        Print("SELECT with NESTED SUBQUERIES", sql, parameters);
    }

    // [Test]
    // public void Select_WithAggregatedSubquery()
    // {
    //     var subQuery = _db
    //         .Select(Avg(UserProjectsTable.Allocation))
    //         .From(userProjects)
    //         .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));
    //
    //     var query = _db
    //         .Select(
    //             UsersTable.Id,
    //             UsersTable.Name,
    //             subQuery.As("AvgAllocation")
    //         )
    //         .From(users)
    //         .Where(Eq(UsersTable.IsActive, true));
    //
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with AGGREGATED SUBQUERY", sql, parameters);
    // }
}
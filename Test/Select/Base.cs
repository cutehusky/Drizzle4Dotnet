using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using SharedDemo;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;
using static Drizzle4Dotnet.Core.Shared.Operators.Functions;
using ProjectSelect = SharedDemo.ProjectSelect;
using UserSelect = SharedDemo.UserSelect;
using UserWithRelationsSelect = SharedDemo.UserWithRelationsSelect;

namespace Test.Select;

[TestFixture]
public class SelectQueryPgTests
{
    private QueryBuilder<PgSqlSqlDialectImpl> _db;

    private UsersTable users;
    private DepartmentsTable departments;
    private RolesTable roles;
    private ManagersTable managers;
    private ProjectsTable projects;
    private UserProjectsTable userProjects;

    [SetUp]
    public void Setup()
    {
        _db = new QueryBuilder<PgSqlSqlDialectImpl>();

        users = new UsersTable();
        departments = new DepartmentsTable();
        roles = new RolesTable();
        managers = new ManagersTable();
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
    public void Select_Basic()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();

        Print("Basic SELECT", sql, parameters);
    }

    [Test]
    public void Select_WithWhere()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(And(
                UsersTable.Id.Eq(1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();

        Print("SELECT with WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithJoin()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELECT with JOIN", sql, parameters);
    }

    [Test]
    public void Select_WithMultipleJoins()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(roles,
                Eq(UsersTable.RoleId, RolesTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELECT with MULTIPLE JOINS", sql, parameters);
    }

    [Test]
    public void Select_SelfJoin_Alias()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .InnerJoin(managers,
                Eq(UsersTable.ManagerId, ManagersTable.Id));

        var (sql, parameters) = query.Build();

        Print("SELF JOIN (Alias)", sql, parameters);
    }

    [Test]
    public void Select_ManyToMany()
    {
        var query = _db
            .Select(ProjectSelect.Record)
            .From(userProjects)
            .InnerJoin(projects,
                Eq(UserProjectsTable.ProjectId, ProjectsTable.Id))
            .InnerJoin(users,
                Eq(UserProjectsTable.UserId, UsersTable.Id));

        var (sql, parameters) = query.Build();

        Print("MANY-TO-MANY JOIN", sql, parameters);
    }

    [Test]
    public void Select_ComplexWhere()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(
                Eq(UsersTable.IsActive, true),
                Eq(UsersTable.Age, 30),
                Like(UsersTable.Email, "%@company.com")
            );

        var (sql, parameters) = query.Build();

        Print("COMPLEX WHERE", sql, parameters);
    }

    [Test]
    public void Select_NullCheck()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(IsNull(UsersTable.ManagerId));

        var (sql, parameters) = query.Build();

        Print("NULL CHECK", sql, parameters);
    }

    [Test]
    public void Select_FullComplex()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(managers,
                Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(And(
                Eq(DepartmentsTable.Id, 1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();

        Print("FULL COMPLEX QUERY", sql, parameters);
    }
    
    [Test]
    public void Select_WithOrderBy()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Name)
            .OrderBy(UsersTable.Age, false);

        var (sql, parameters) = query.Build();

        Print("SELECT with ORDER BY", sql, parameters);
    }

    [Test]
    public void Select_WithLimitOffset()
    {
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Id)
            .Limit(10)
            .Offset(20);

        var (sql, parameters) = query.Build();

        Print("SELECT with LIMIT/OFFSET", sql, parameters);
    }

    [Test]
    public void Select_WithGroupBy()
    {
        var query = _db
            .Select(
                DepartmentsTable.Id,
                DepartmentsTable.Name,
                Count(UsersTable.Id).As("UserCount")
            )
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .GroupBy(DepartmentsTable.Id, DepartmentsTable.Name)
            .Having(Gt(Count(UsersTable.Id), 5));
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with GROUP BY and HAVING", sql, parameters);
    }

    [Test]
    public void Select_WithDistinct()
    {
        var query = _db
            .SelectDistinct(UserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();

        Print("SELECT DISTINCT", sql, parameters);
    }

    [Test]
    public void Select_WithSubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));
    
        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, subQuery));
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with SUBQUERY", sql, parameters);
    }

    [Test]
    public void Select_WithComplexJoinsAndWhere()
    {
        var query = _db
            .Select(UserWithRelationsSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id))
            .LeftJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(
                Eq(UsersTable.IsActive, true),
                Gt(UsersTable.Age, 25),
                Like(UsersTable.Email, "%@company.com")
            )
            .OrderBy(UsersTable.Name)
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
    
    [Test]
    public void Select_WithAggregateFunctions()
    {
        var query = _db
            .Select(
                Count(UsersTable.Id).As("TotalUsers"),
                Avg(UsersTable.Age).As("AverageAge"),
                Max(UsersTable.Salary).As("MaxSalary"),
                Min(UsersTable.Salary).As("MinSalary")
            )
            .From(users);
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with AGGREGATE functions", sql, parameters);
    }
    
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
            .Select(UserProjectsTable.Id)
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
    // public void Select_WithInSubqueryAndValues()
    // {
    //     var subQuery = _db.Select(DepartmentsTable.ManagerId)
    //         .From(departments)
    //         .Where(Eq(DepartmentsTable.Name, "Engineering"));
    //
    //     var query = _db.Select(UsersTable.Id)
    //         .From(users)
    //         .Where(In<int, PgSqlSqlDialectImpl>(UsersTable.Id, 10, 20, subQuery));
    //     
    //     
    //     var (sql, parameters) = query.Build();
    //
    //     Print("SELECT with IN SUBQUERY and VALUES", sql, parameters);
    // }

    [Test]
    public void Select_FromSubquery_AsDerivedTable()
    {
        var subQuery = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true)).AsSubQuery("active_users");
    
        var query = _db
            .Select(subQuery.Id, subQuery.Email)
            .From(subQuery);
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT FROM SUBQUERY (Derived Table)", sql, parameters);
    }

    [Test]
    public void Select_SubqueryInSelectList()
    {
        var subQuery = _db
            .Select(Count(UserProjectsTable.ProjectId).As("ProjectCount"))
            .From(userProjects)
            .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));
    
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                subQuery.As("ProjectCount")
            )
            .From(users);
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with SUBQUERY in SELECT list", sql, parameters);
    }

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
            .Select(UserProjectsTable.UserId)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 3));

        var outerSub = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(In(UsersTable.Id, innerSub));

        var query = _db
            .Select(UserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, outerSub));

        var (sql, parameters) = query.Build();

        Print("SELECT with NESTED SUBQUERIES", sql, parameters);
    }

    [Test]
    public void Select_WithAggregatedSubquery()
    {
        var subQuery = _db
            .Select(Avg(UserProjectsTable.Allocation).As("AvgAllocation"))
            .From(userProjects)
            .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));
    
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                subQuery.As("AvgAllocation")
            )
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));
    
        var (sql, parameters) = query.Build();
    
        Print("SELECT with AGGREGATED SUBQUERY", sql, parameters);
    }

    [Test]
    public void Select_ProjectBudgetVsDeptTotal()
    {
        var deptBudgets = _db
            .Select(DepartmentsTable.Id, Sum(ProjectsTable.Budget).As("TotalDeptBudget"))
            .From(projects)
            .GroupBy(ProjectsTable.DepartmentId)
            .AsSubQuery("dept_budgets");
    
        var query = _db
            .Select(ProjectsTable.Name, ProjectsTable.Budget, deptBudgets.Field<decimal>("TotalDeptBudget"))
            .With(deptBudgets)
            .From(projects)
            .InnerJoin(deptBudgets, Eq(ProjectsTable.DepartmentId, deptBudgets.Field<int>("Id")))
            .Where(Gt(ProjectsTable.Budget, Mul(deptBudgets.Field<decimal>("TotalDeptBudget"), 0.1m)));
    
        var (sql, parameters) = query.Build();
        Print("CTE with Aggregation", sql, parameters);
    }
    
    [Test]
    public void Select_ActiveUsersInActiveProjects()
    {
        var activeUsers = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_users");
    
        var projectMembers = _db
            .Select(
                UserProjectsTable.UserId, 
                ProjectsTable.Name.As("ProjectName")
                )
            .From(projects)
            .InnerJoin(userProjects, Eq(ProjectsTable.Id, UserProjectsTable.ProjectId))
            .Where(Eq(ProjectsTable.IsActive, true))
            .AsSubQuery("active_project_members");
    
        var query = _db
            .Select(activeUsers.Name, projectMembers.Field<string>("ProjectName"))
            .With(activeUsers)
            .With(projectMembers)
            .From(activeUsers)
            .InnerJoin(projectMembers, Eq(activeUsers.Id, projectMembers.Field<int>("UserId")));
    
        var (sql, parameters) = query.Build();
        Print("Multiple CTEs Join", sql, parameters);
    }
    //
    // [Test]
    // public void Select_DepartmentHierarchy_Recursive()
    // {
    //     var deptTree = _db
    //         .Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
    //         .From(departments)
    //         .Where(Eq(DepartmentsTable.Id, 1))
    //         .UnionAll(
    //             _db.Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
    //                 .From(departments)
    //                 .InnerJoin("dept_tree", Eq(DepartmentsTable.ParentDepartmentId, Field("dept_tree", "Id")))
    //         )
    //         .AsRecursiveSubQuery("dept_tree");
    //
    //     var query = _db
    //         .SelectAll()
    //         .With(deptTree)
    //         .From(deptTree);
    //
    //     var (sql, parameters) = query.Build();
    //     Print("Recursive Department Tree", sql, parameters);
    // }
    
    [Test]
    public void Select_SalaryGapWithManager()
    {
        var managerSalaries = _db
            .Select(UsersTable.Id, UsersTable.Salary)
            .From(users)
            .AsSubQuery("m_salary");
    
        var query = _db
            .Select(
                UsersTable.Name, 
                UsersTable.Salary.As("UserSalary"),
                managerSalaries.Field<decimal>("Salary").As("ManagerSalary"),
                Sub(UsersTable.Salary, managerSalaries.Field<decimal>("Salary")).As("Gap")
            )
            .With(managerSalaries)
            .From(users)
            .InnerJoin(managerSalaries, Eq(UsersTable.ManagerId, managerSalaries.Field<decimal>("Id")));
    
        var (sql, parameters) = query.Build();
        Print("CTE Salary Gap Analysis", sql, parameters);
    }
    
    
    [Test]
    public void Select_HighLevelRoleUsers()
    {
        var highRoles = _db
            .Select(RolesTable.Id)
            .From(roles)
            .Where(Gt(RolesTable.Level, 5))
            .AsSubQuery("high_roles");
    
        var query = _db
            .Select(UsersTable.Name, UsersTable.Email)
            .With(highRoles)
            .From(users)
            .Where(In(UsersTable.RoleId, highRoles.Field<int>("Id")));
    
        var (sql, parameters) = query.Build();
        Print("CTE as Filter Scope", sql, parameters);
    }
}
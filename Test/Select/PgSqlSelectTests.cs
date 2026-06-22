using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.PgSql;
using Drizzle4Dotnet.PgSql.Nodes;
using SharedDemo.PgSql;
using static Drizzle4Dotnet.Core.Shared.Operators.Operators;
using static Drizzle4Dotnet.Core.Shared.Operators.Functions;
using PgUserSelect = SharedDemo.PgSql.UserSelect;
using PgUserWithRelationsSelect = SharedDemo.PgSql.UserWithRelationsSelect;
using PgProjectSelect = SharedDemo.PgSql.ProjectSelect;

namespace Test.Select;

[TestFixture]
public class PgSqlSelectTests
{
    private PgSqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private DepartmentsTable departments = null!;
    private RolesTable roles = null!;
    private ManagersTable managers = null!;
    private ProjectsTable projects = null!;
    private UserProjectsTable userProjects = null!;

    [SetUp]
    public void Setup()
    {
        _db = new PgSqlQueryBuilder();

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

    // =========================================================================
    // 1. BASIC SELECT
    // =========================================================================

    [Test]
    public void Select_Basic()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Basic SELECT", sql, parameters);
    }

    [Test]
    public void Select_WithWhere()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(And(
                UsersTable.Id.Eq(1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithJoin()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with JOIN", sql, parameters);
    }

    [Test]
    public void Select_WithMultipleJoins()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with MULTIPLE JOINS", sql, parameters);
    }

    [Test]
    public void Select_SelfJoin_Alias()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELF JOIN (Alias)", sql, parameters);
    }

    [Test]
    public void Select_ManyToMany()
    {
        var query = _db
            .Select(PgProjectSelect.Record)
            .From(userProjects)
            .InnerJoin(projects, Eq(UserProjectsTable.ProjectId, ProjectsTable.Id))
            .InnerJoin(users, Eq(UserProjectsTable.UserId, UsersTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL MANY-TO-MANY JOIN", sql, parameters);
    }

    [Test]
    public void Select_ComplexWhere()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(
                Eq(UsersTable.IsActive, true),
                Eq(UsersTable.Age, 30),
                Like(UsersTable.Email, "%@company.com")
            );

        var (sql, parameters) = query.Build();
        Print("PgSQL COMPLEX WHERE", sql, parameters);
    }

    [Test]
    public void Select_FullComplex()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(And(
                Eq(DepartmentsTable.Id, 1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL FULL COMPLEX QUERY", sql, parameters);
    }

    // =========================================================================
    // 2. ORDER BY / LIMIT / OFFSET / DISTINCT
    // =========================================================================

    [Test]
    public void Select_WithOrderBy()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Name)
            .OrderBy(UsersTable.Age, false);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with ORDER BY", sql, parameters);
    }

    [Test]
    public void Select_WithLimitOffset()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Id)
            .Limit(10)
            .Offset(20);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with LIMIT/OFFSET", sql, parameters);
    }

    [Test]
    public void Select_WithDistinct()
    {
        var query = _db
            .SelectDistinct(PgUserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT DISTINCT", sql, parameters);
    }

    // =========================================================================
    // 3. AGGREGATE FUNCTIONS + GROUP BY + HAVING
    // =========================================================================

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
        Print("PgSQL SELECT with GROUP BY and HAVING", sql, parameters);
    }

    [Test]
    public void Select_WithAggregateFunctions()
    {
        var query = _db
            .Select(
                Count(UsersTable.Id).As("TotalUsers"),
                Avg(UsersTable.Age).As("AverageAge"),
                Max(UsersTable.Salary).As("MaxSalary"),
                Min(UsersTable.Salary).As("MinSalary"),
                Sum(UsersTable.Salary).As("TotalSalary"),
                StdDev(UsersTable.Salary).As("StdSalary"),
                Variance(UsersTable.Salary).As("VarSalary"),
                CountDistinct(UsersTable.Email).As("UniqueEmails"),
                StdDevSample(UsersTable.Salary).As("StdDevSamp"),
                StdDevPop(UsersTable.Salary).As("StdDevPop"),
                VarSample(UsersTable.Salary).As("VarSamp"),
                VarPop(UsersTable.Salary).As("VarPop")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with AGGREGATE functions", sql, parameters);
    }

    // =========================================================================
    // 4. COMPARISON / LOGICAL / STRING OPERATORS
    // =========================================================================

    [Test]
    public void Select_ComparisonOperators()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(And(
                Eq(UsersTable.Id, 1),
                Lt(UsersTable.Age, 30),
                Gt(UsersTable.Age, 18),
                Ltq(UsersTable.Salary, 100000m),
                Gtq(UsersTable.Rating, 3.0),
                Ne(UsersTable.Name, "Admin")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL Comparison Operators", sql, parameters);
    }

    [Test]
    public void Select_LogicalOperators()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(And(
                Or(
                    Eq(UsersTable.IsActive, true),
                    Eq(UsersTable.IsActive, false)
                ),
                Not(Eq(UsersTable.Name, "Test"))
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL Logical Operators (AND, OR, NOT)", sql, parameters);
    }

    [Test]
    public void Select_NullCheck()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(IsNull(UsersTable.ManagerId));

        var (sql, parameters) = query.Build();
        Print("PgSQL NULL CHECK", sql, parameters);
    }

    [Test]
    public void Select_IsNotNull()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(IsNotNull(UsersTable.ManagerId));

        var (sql, parameters) = query.Build();
        Print("PgSQL IS NOT NULL", sql, parameters);
    }

    [Test]
    public void Select_StringOperators()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(And(
                Like(UsersTable.Email, "%@example.com"),
                NotLike(UsersTable.Email, "test%@"),
                Contains(UsersTable.Name, "John"),
                StartsWith(UsersTable.Name, "J"),
                EndsWith(UsersTable.Email, ".com")
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL String Operators", sql, parameters);
    }

    [Test]
    public void Select_InOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, [1L, 2L, 3L, 4L, 5L]));

        var (sql, parameters) = query.Build();
        Print("PgSQL IN Operator with list", sql, parameters);
    }

    [Test]
    public void Select_NotInOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(NotIn(UsersTable.Id, [1L, 2L, 3L, 4L, 5L]));

        var (sql, parameters) = query.Build();
        Print("PgSQL NOT IN Operator", sql, parameters);
    }

    [Test]
    public void Select_InValuesList()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(In<long, PgSqlSqlDialectImpl>(UsersTable.Id, 1L, 2L, 3L, 4L, 5L));

        var (sql, parameters) = query.Build();
        Print("PgSQL IN with typed values list", sql, parameters);
    }

    [Test]
    public void Select_BetweenOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Between(UsersTable.Age, 18, 65));

        var (sql, parameters) = query.Build();
        Print("PgSQL BETWEEN Operator", sql, parameters);
    }

    [Test]
    public void Select_NotBetweenOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(NotBetween(UsersTable.Age, 18, 25));

        var (sql, parameters) = query.Build();
        Print("PgSQL NOT BETWEEN Operator", sql, parameters);
    }

    [Test]
    public void Select_ArithmeticOperators()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                Add(UsersTable.Salary, 1000m).As("Raise"),
                Sub(UsersTable.Salary, 500m).As("Deduction"),
                Mul(UsersTable.Salary, 1.1m).As("Increased"),
                Div(UsersTable.Salary, 12).As("Monthly"),
                Mod(UsersTable.Age, 10).As("AgeMod")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Arithmetic Operators", sql, parameters);
    }

    [Test]
    public void Select_ConcatOperator()
    {
        var query = _db
            .Select(
                Concat(UsersTable.Name, Sql.Value(" - ")).As("NameWithDash")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Concat (||) Operator", sql, parameters);
    }

    // =========================================================================
    // 5. PGSQL-SPECIFIC OPERATORS
    // =========================================================================

    [Test]
    public void Select_IsDistinctFrom()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.IsDistinctFrom(UsersTable.ManagerId, null));

        var (sql, parameters) = query.Build();
        Print("PgSQL IS DISTINCT FROM", sql, parameters);
    }

    [Test]
    public void Select_IsNotDistinctFrom()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.IsNotDistinctFrom(UsersTable.ManagerId, null));

        var (sql, parameters) = query.Build();
        Print("PgSQL IS NOT DISTINCT FROM", sql, parameters);
    }

    [Test]
    public void Select_IsDistinctFromColumn()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.IsDistinctFrom(UsersTable.ManagerId, DepartmentsTable.ManagerId));

        var (sql, parameters) = query.Build();
        Print("PgSQL IS DISTINCT FROM (column-column)", sql, parameters);
    }

    [Test]
    public void Select_AnySubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.Any(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("PgSQL = ANY (subquery)", sql, parameters);
    }

    [Test]
    public void Select_AllSubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.All(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("PgSQL = ALL (subquery)", sql, parameters);
    }

    [Test]
    public void Select_SomeSubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.Some(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("PgSQL = SOME (subquery)", sql, parameters);
    }

    // =========================================================================
    // 6. SUBQUERIES
    // =========================================================================

    [Test]
    public void Select_WithSubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with SUBQUERY", sql, parameters);
    }

    [Test]
    public void Select_WithExists()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 1));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Exists(subQuery));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with EXISTS", sql, parameters);
    }

    [Test]
    public void Select_FromSubquery_DerivedTable()
    {
        var subQuery = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true)).AsSubQuery("active_users");

        var query = _db
            .Select(subQuery.Id, subQuery.Email)
            .From(subQuery);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT FROM SUBQUERY (Derived Table)", sql, parameters);
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
        Print("PgSQL SELECT with SUBQUERY in SELECT list", sql, parameters);
    }

    [Test]
    public void Select_ComplexExistsAndJoins()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));

        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(And(
                Exists(subQuery),
                Eq(DepartmentsTable.Name, "Engineering")
            ))
            .OrderBy(UsersTable.Name);

        var (sql, parameters) = query.Build();
        Print("PgSQL COMPLEX SELECT with EXISTS and JOIN", sql, parameters);
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
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, outerSub));

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with NESTED SUBQUERIES", sql, parameters);
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
        Print("PgSQL SELECT with AGGREGATED SUBQUERY", sql, parameters);
    }

    // =========================================================================
    // 7. CTE (Common Table Expressions)
    // =========================================================================

    [Test]
    public void Select_WithCTE()
    {
        var activeUsers = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_users").AsCte();

        var query = _db
            .Select(activeUsers.Name, activeUsers.Email)
            .With(activeUsers)
            .From(activeUsers);

        var (sql, parameters) = query.Build();
        Print("PgSQL CTE", sql, parameters);
    }

    [Test]
    public void Select_CTE_SalaryGap()
    {
        var managerSalaries = _db
            .Select(UsersTable.Id, UsersTable.Salary)
            .From(users)
            .AsSubQuery("m_salary").AsCte();

        var query = _db
            .Select(
                UsersTable.Name,
                UsersTable.Salary.As("UserSalary"),
                managerSalaries.Field<decimal>("Salary").As("ManagerSalary"),
                Sub(UsersTable.Salary, managerSalaries.Field<decimal>("Salary")).As("Gap")
            )
            .With(managerSalaries)
            .From(users)
            .InnerJoin(managerSalaries, Eq(UsersTable.ManagerId, managerSalaries.Field<long>("Id")));

        var (sql, parameters) = query.Build();
        Print("PgSQL CTE Salary Gap", sql, parameters);
    }

    [Test]
    public void Select_CTE_FilterScope()
    {
        var highRoles = _db
            .Select(RolesTable.Id)
            .From(roles)
            .Where(Gt(RolesTable.Level, 5))
            .AsSubQuery("high_roles").AsCte();

        var query = _db
            .Select(UsersTable.Name, UsersTable.Email)
            .With(highRoles)
            .From(users)
            .Where(In(UsersTable.RoleId, highRoles.Field<long>("Id")));

        var (sql, parameters) = query.Build();
        Print("PgSQL CTE as Filter Scope", sql, parameters);
    }

    [Test]
    public void Select_MultipleCTEs()
    {
        var activeUsers = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_users").AsCte();

        var projectMembers = _db
            .Select(UserProjectsTable.UserId, ProjectsTable.Name)
            .From(projects)
            .InnerJoin(userProjects, Eq(ProjectsTable.Id, UserProjectsTable.ProjectId))
            .Where(Eq(ProjectsTable.IsActive, true))
            .AsSubQuery("active_project_members", (from) => new
            {
                UserId = from.Field(UserProjectsTable.UserId),
                ProjectName = from.Field(ProjectsTable.Name)
            }).AsCte();

        var query = _db
            .Select(activeUsers.Name, projectMembers.Shape.ProjectName)
            .With(activeUsers)
            .With(projectMembers)
            .From(activeUsers)
            .InnerJoin(projectMembers, Eq(activeUsers.Id, projectMembers.Shape.UserId));

        var (sql, parameters) = query.Build();
        Print("PgSQL Multiple CTEs Join", sql, parameters);
    }

    [Test]
    public void Select_CTE_ProjectBudgetVsDeptTotal()
    {
        var deptBudgets = _db
            .Select(DepartmentsTable.Id, Sum(ProjectsTable.Budget).As("TotalDeptBudget"))
            .From(projects)
            .GroupBy(ProjectsTable.DepartmentId)
            .AsSubQuery("dept_budgets", (from) => new
            {
                Id = from.Field<long>("Id"),
                TotalDeptBudget = from.Field<decimal>("TotalDeptBudget")
            }).AsCte();

        var query = _db
            .Select(ProjectsTable.Name, ProjectsTable.Budget, deptBudgets.Shape.TotalDeptBudget)
            .With(deptBudgets)
            .From(projects)
            .InnerJoin(deptBudgets, Eq(ProjectsTable.DepartmentId, deptBudgets.Shape.Id))
            .Where(Gt(ProjectsTable.Budget, Mul(deptBudgets.Shape.TotalDeptBudget, 0.1m)));

        var (sql, parameters) = query.Build();
        Print("PgSQL CTE with Aggregation", sql, parameters);
    }

    // =========================================================================
    // 8. CASE EXPRESSION
    // =========================================================================

    [Test]
    public void Select_WithCaseExpression()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                Case
                    .When(Eq(UsersTable.IsActive, true), Sql.Value("Active"))
                    .Else(Sql.Value("Inactive")).Build().As("Status")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL SELECT with CASE expression", sql, parameters);
    }

    [Test]
    public void Select_WithCaseMultiWhen()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Age,
                Case
                    .When(Lt(UsersTable.Age, 18), Sql.Value("Minor"))
                    .When(Between(UsersTable.Age, 18, 65), Sql.Value("Adult"))
                    .Else(Sql.Value("Senior")).Build().As("AgeGroup")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL CASE with multiple WHEN", sql, parameters);
    }

    // =========================================================================
    // 9. STANDARD FUNCTIONS (String, Numeric, DateTime, Conditional, Cast)
    // =========================================================================

    [Test]
    public void Select_StandardStringFunctions()
    {
        var query = _db
            .Select(
                Upper(UsersTable.Name).As("UpperName"),
                Lower(UsersTable.Email).As("LowerEmail"),
                Trim(UsersTable.Name).As("Trimmed"),
                LTrim(UsersTable.Name).As("LTrimmed"),
                RTrim(UsersTable.Name).As("RTrimmed"),
                Length(UsersTable.Name).As("NameLength"),
                Substring(UsersTable.Email, 1, 5).As("SubEmail"),
                Replace(UsersTable.Email, "@example.com", "@company.com").As("ReplacedEmail")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Standard String Functions", sql, parameters);
    }

    [Test]
    public void Select_StandardNumericFunctions()
    {
        var query = _db
            .Select(
                Abs(UsersTable.Salary).As("AbsSalary"),
                Ceil(UsersTable.Rating).As("CeilRating"),
                Floor(UsersTable.Rating).As("FloorRating"),
                Round(UsersTable.Rating).As("RoundRating"),
                Round(UsersTable.Rating, 1).As("RoundRating1"),
                Power(UsersTable.Salary, 2).As("Squared"),
                Sqrt(UsersTable.Rating).As("SqrtRating"),
                Sign(UsersTable.Rating).As("SignRating")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Standard Numeric Functions", sql, parameters);
    }

    [Test]
    public void Select_StandardDateTimeFunctions()
    {
        var query = _db
            .Select(
                Now().As("Now"),
                CurrentTimestamp().As("Ts"),
                CurrentDate().As("Today")
            )
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("PgSQL Standard DateTime Functions", sql, parameters);
    }

    [Test]
    public void Select_ConditionalFunctions()
    {
        var query = _db
            .Select(
                Coalesce(UsersTable.ManagerId, 0).As("ManagerIdOrDefault"),
                NullIf(UsersTable.Name, "Admin").As("NameOrNull")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Conditional Functions", sql, parameters);
    }

    [Test]
    public void Select_CastFunctions()
    {
        var query = _db
            .Select(
                Cast<string>(UsersTable.Id, "TEXT").As("IdStr"),
                CastToString(UsersTable.Age).As("AgeStr"),
                CastToInt(UsersTable.Rating).As("RatingInt"),
                CastToDouble(UsersTable.Salary).As("SalaryDbl"),
                CastToDateTime(UsersTable.Id).As("DateTimeCast")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL CAST Functions (standard)", sql, parameters);
    }

    // =========================================================================
    // 10. PGSQL-SPECIFIC STRING FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_PgPosition()
    {
        var query = _db
            .Select(
                PgFunctions.Position(UsersTable.Email, "@").As("AtPos")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL POSITION", sql, parameters);
    }

    [Test]
    public void Select_PgConcatWs()
    {
        var query = _db
            .Select(
                PgFunctions.ConcatWs(", ", UsersTable.Id, UsersTable.Name, UsersTable.Email).As("Info")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL CONCAT_WS", sql, parameters);
    }

    // =========================================================================
    // 11. PGSQL DATE/TIME FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_PgExtract()
    {
        var query = _db
            .Select(
                PgFunctions.Extract("YEAR", UsersTable.CreatedAt).As("Year"),
                PgFunctions.Extract("MONTH", UsersTable.CreatedAt).As("Month"),
                PgFunctions.Extract("DAY", UsersTable.CreatedAt).As("Day")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL EXTRACT", sql, parameters);
    }

    [Test]
    public void Select_PgDateTrunc()
    {
        var query = _db
            .Select(
                PgFunctions.DateTrunc("month", UsersTable.CreatedAt).As("MonthStart")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL DATE_TRUNC", sql, parameters);
    }

    [Test]
    public void Select_PgDateAdd()
    {
        var query = _db
            .Select(
                PgFunctions.DateAdd(UsersTable.CreatedAt, 7, "days").As("Plus7Days")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL DateAdd (INTERVAL)", sql, parameters);
    }

    [Test]
    public void Select_PgDateDiff()
    {
        var query = _db
            .Select(
                PgFunctions.DateDiff(UsersTable.CreatedAt, 30, "days").As("Minus30Days")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL DateDiff (INTERVAL)", sql, parameters);
    }

    [Test]
    public void Select_PgAtTimeZone()
    {
        var query = _db
            .Select(
                PgFunctions.AtTimeZone(UsersTable.CreatedAt, "UTC").As("UtcTime")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL AT TIME ZONE", sql, parameters);
    }

    [Test]
    public void Select_PgAge()
    {
        var query = _db
            .Select(
                PgFunctions.Age(UsersTable.CreatedAt).As("Age")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL AGE()", sql, parameters);
    }

    // [Test]
    // public void Select_PgAgeTwoDates()
    // {
    //     var query = _db
    //         .Select(
    //             PgFunctions.Age(UsersTable.UpdatedAt ?? UsersTable.CreatedAt, UsersTable.CreatedAt).As("Diff")
    //         )
    //         .From(users);
    //
    //     var (sql, parameters) = query.Build();
    //     Print("PgSQL AGE(date1, date2)", sql, parameters);
    // }

    [Test]
    public void Select_PgIntervalLiteral()
    {
        var query = _db
            .Select(PgSqlStatics.Interval(1, "day").As("OneDay"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("PgSQL INTERVAL literal", sql, parameters);
    }

    // =========================================================================
    // 12. PGSQL JSON FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_PgJsonExtract()
    {
        var query = _db
            .Select(
                PgFunctions.JsonExtract<long, string>(UsersTable.Id, Sql.Value("->'key'")).As("Val")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL JSON -> extract", sql, parameters);
    }

    [Test]
    public void Select_PgJsonExtractText()
    {
        var query = _db
            .Select(
                PgFunctions.JsonExtractText(UsersTable.Id, Sql.Value("->>'name'")).As("Name")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL JSON ->> extract text", sql, parameters);
    }

    [Test]
    public void Select_PgJsonAgg()
    {
        var query = _db
            .Select(
                PgFunctions.JsonAgg(UsersTable.Name).As("Names")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL JSON_AGG", sql, parameters);
    }

    [Test]
    public void Select_PgJsonBuildObject()
    {
        var query = _db
            .Select(
                PgFunctions.JsonBuildObject(
                    Sql.Value("id"), UsersTable.Id,
                    Sql.Value("name"), UsersTable.Name
                ).As("Obj")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL JSON_BUILD_OBJECT", sql, parameters);
    }

    [Test]
    public void Select_PgJsonArrayLength()
    {
        var query = _db
            .Select(
                PgFunctions.JsonArrayLength(UsersTable.Id).As("Len")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL JSON_ARRAY_LENGTH", sql, parameters);
    }

    [Test]
    public void Select_PgToJson()
    {
        var query = _db
            .Select(
                PgFunctions.ToJson(UsersTable.Name).As("JsonName")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL TO_JSON", sql, parameters);
    }

    // [Test]
    // public void Select_PgRowToJson()
    // {
    //     var query = _db
    //         .Select(
    //             PgFunctions.RowToJson(UsersTable.ModelAll).As("RowJson")
    //         )
    //         .From(users);
    //
    //     var (sql, parameters) = query.Build();
    //     Print("PgSQL ROW_TO_JSON", sql, parameters);
    // }

    // =========================================================================
    // 13. PGSQL ARRAY FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_PgArrayAgg()
    {
        var query = _db
            .Select(
                PgFunctions.ArrayAgg(UsersTable.Name).As("Names")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL ARRAY_AGG", sql, parameters);
    }

    [Test]
    public void Select_PgUnnest()
    {
        var query = _db
            .Select(
                PgFunctions.Unnest(UsersTable.Id).As("Id")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL UNNEST", sql, parameters);
    }

    [Test]
    public void Select_PgArrayLength()
    {
        var query = _db
            .Select(
                PgFunctions.ArrayLength(UsersTable.Id, 1).As("ArrLen")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL ARRAY_LENGTH", sql, parameters);
    }

    // =========================================================================
    // 14. PGSQL OTHER FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_PgRandom()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                PgFunctions.Random<double>().As("Random")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL RANDOM()", sql, parameters);
    }

    [Test]
    public void Select_PgCastPg()
    {
        var query = _db
            .Select(
                PgFunctions.CastPg<string>(UsersTable.Id, "TEXT").As("IdStr")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL PostgreSQL-style CAST (::)", sql, parameters);
    }

    // =========================================================================
    // 15. WINDOW FUNCTIONS
    // =========================================================================

    [Test]
    public void Select_Window_RowNumber()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                UsersTable.DepartmentId,
                PgFunctions.RowNumber()
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId)
                        .OrderBy(UsersTable.Salary, false))
                    .As("RowNum")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL ROW_NUMBER() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_Rank()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                UsersTable.Salary,
                PgFunctions.Rank()
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Salary, false))
                    .As("Rank")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL RANK() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_DenseRank()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                UsersTable.DepartmentId,
                PgFunctions.DenseRank()
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId)
                        .OrderBy(UsersTable.Salary, false))
                    .As("DenseRank")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL DENSE_RANK() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_Ntile()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                UsersTable.Salary,
                PgFunctions.Ntile(4)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Salary, false))
                    .As("Quartile")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL NTILE(4) OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_Lead()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                PgFunctions.Lead(UsersTable.Name, 1)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Id))
                    .As("NextName")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL LEAD() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_LeadWithDefault()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Salary,
                PgFunctions.Lead(UsersTable.Salary, 1, Sql.Value(0m))
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Id))
                    .As("NextSalary")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL LEAD() with default OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_Lag()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Name,
                PgFunctions.Lag(UsersTable.Name, 1)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Id))
                    .As("PrevName")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL LAG() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_FirstValue()
    {
        var query = _db
            .Select(
                UsersTable.DepartmentId,
                UsersTable.Name,
                UsersTable.Salary,
                PgFunctions.FirstValue(UsersTable.Name)
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId)
                        .OrderBy(UsersTable.Salary, false))
                    .As("HighestPaid")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL FIRST_VALUE() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_LastValue()
    {
        var query = _db
            .Select(
                UsersTable.DepartmentId,
                UsersTable.Name,
                UsersTable.Salary,
                PgFunctions.LastValue(UsersTable.Name)
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId)
                        .OrderBy(UsersTable.Salary, false))
                    .As("LowestPaid")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL LAST_VALUE() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_NthValue()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Salary,
                PgFunctions.NthValue(UsersTable.Salary, 3)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Salary, false))
                    .As("ThirdHighest")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL NTH_VALUE() OVER()", sql, parameters);
    }

    [Test]
    public void Select_Window_AggregateOver()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.DepartmentId,
                UsersTable.Salary,
                Sum(UsersTable.Salary)
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId))
                    .As("DeptTotal"),
                Avg(UsersTable.Salary)
                    .Over(PgOver.Create()
                        .PartitionBy(UsersTable.DepartmentId))
                    .As("DeptAvg")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Aggregate OVER(PARTITION BY)", sql, parameters);
    }

    [Test]
    public void Select_Window_RowsBetween()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Salary,
                Sum(UsersTable.Salary)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Id)
                        .RowsBetween(
                            PgWindowFrameBoundary.UnboundedPreceding,
                            PgWindowFrameBoundary.CurrentRow))
                    .As("RunningTotal")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Running Total ROWS BETWEEN", sql, parameters);
    }

    [Test]
    public void Select_Window_RangeBetween()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Salary,
                Avg(UsersTable.Salary)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Salary)
                        .RangeBetween(
                            PgWindowFrameBoundary.Preceding(1000),
                            PgWindowFrameBoundary.Following(1000)))
                    .As("MovingAvg")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Moving Average RANGE BETWEEN", sql, parameters);
    }

    [Test]
    public void Select_Window_GroupsBetween()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                UsersTable.Salary,
                Count(UsersTable.Id)
                    .Over(PgOver.Create()
                        .OrderBy(UsersTable.Salary)
                        .GroupsBetween(
                            PgWindowFrameBoundary.UnboundedPreceding,
                            PgWindowFrameBoundary.UnboundedFollowing))
                    .As("TotalCount")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL GROUPS BETWEEN frame", sql, parameters);
    }

    // =========================================================================
    // 16. FILTERED AGGREGATES
    // =========================================================================

    [Test]
    public void Select_FilteredAggregate()
    {
        var query = _db
            .Select(
                Count(UsersTable.Id).Filter(Eq(UsersTable.IsActive, true)).As("ActiveCount"),
                Avg(UsersTable.Salary).Filter(Gt(UsersTable.Age, 30)).As("AvgSalaryOver30"),
                Sum(UsersTable.Salary).Filter(Eq(UsersTable.DepartmentId, 1)).As("Dept1Salary"),
                Max(UsersTable.Salary).Filter(Eq(UsersTable.IsActive, true)).As("MaxActiveSalary")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL FILTERED AGGREGATES", sql, parameters);
    }

    // =========================================================================
    // 17. JOIN TYPES
    // =========================================================================

    [Test]
    public void Select_AllJoinTypes()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .LeftJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id))
            .RightJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .FullJoin(projects, Eq(UsersTable.DepartmentId, ProjectsTable.DepartmentId))
            .CrossJoin(userProjects);

        var (sql, parameters) = query.Build();
        Print("PgSQL All Join Types", sql, parameters);
    }

    [Test]
    public void Select_ComplexJoinsAndWhere()
    {
        var query = _db
            .Select(PgUserWithRelationsSelect.Record)
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
        Print("PgSQL COMPLEX JOIN + WHERE + ORDER + LIMIT", sql, parameters);
    }

    // =========================================================================
    // 18. LATERAL JOINS (PgSQL-specific)
    // =========================================================================

    [Test]
    public void Select_InnerLateralJoin()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .InnerLateralJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL INNER LATERAL JOIN", sql, parameters);
    }

    [Test]
    public void Select_LeftLateralJoin()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .LeftLateralJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("PgSQL LEFT LATERAL JOIN", sql, parameters);
    }

    [Test]
    public void Select_CrossLateralJoin()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .CrossLateralJoin(departments);

        var (sql, parameters) = query.Build();
        Print("PgSQL CROSS LATERAL JOIN", sql, parameters);
    }

    // =========================================================================
    // 19. LOCKING
    // =========================================================================

    [Test]
    public void Select_ForUpdate()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForUpdate();

        var (sql, parameters) = query.Build();
        Print("PgSQL FOR UPDATE", sql, parameters);
    }

    [Test]
    public void Select_ForShare()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForShare();

        var (sql, parameters) = query.Build();
        Print("PgSQL FOR SHARE", sql, parameters);
    }

    [Test]
    public void Select_ForNoKeyUpdate()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForNoKeyUpdate();

        var (sql, parameters) = query.Build();
        Print("PgSQL FOR NO KEY UPDATE", sql, parameters);
    }

    [Test]
    public void Select_ForKeyShare()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForKeyShare();

        var (sql, parameters) = query.Build();
        Print("PgSQL FOR KEY SHARE", sql, parameters);
    }

    // =========================================================================
    // 20. RAW SQL / UTILITY
    // =========================================================================

    [Test]
    public void Select_WithRawSQL()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Sql.Raw("\"Age\" > 18"));

        var (sql, parameters) = query.Build();
        Print("PgSQL Raw SQL WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithLiteral()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                Sql.Literal("'constant_value'").As("ConstVal")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL Literal SQL", sql, parameters);
    }

    [Test]
    public void Select_WithNullValue()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                Sql.Null<string>().As("NullCol")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL NULL value in SELECT", sql, parameters);
    }

    [Test]
    public void Select_WithXorOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Xor(
                Eq(UsersTable.IsActive, true),
                Gt(UsersTable.Rating, 4.0)
            ));

        var (sql, parameters) = query.Build();
        Print("PgSQL XOR Operator", sql, parameters);
    }

    // =========================================================================
    // 21. RECURSIVE CTE (via compound query)
    // =========================================================================

    [Test]
    public void Select_WithCompoundQuery_Union()
    {
        var active = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var inactive = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Eq(UsersTable.IsActive, false));

        var compound = active.Union(inactive);

        var (sql, parameters) = compound.Build();
        Print("PgSQL UNION compound query", sql, parameters);
    }

    [Test]
    public void Select_WithCompoundQuery_UnionAll()
    {
        var adults = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Gt(UsersTable.Age, 18));

        var minors = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Ltq(UsersTable.Age, 18));

        var compound = adults.UnionAll(minors);

        var (sql, parameters) = compound.Build();
        Print("PgSQL UNION ALL compound query", sql, parameters);
    }

    [Test]
    public void Select_WithCompoundQuery_Intersect()
    {
        var q1 = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Gt(UsersTable.Age, 25));

        var q2 = _db
            .Select(UserProjectsTable.UserId)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.IsActive, true));

        var compound = q1.Intersect(q2);

        var (sql, parameters) = compound.Build();
        Print("PgSQL INTERSECT compound query", sql, parameters);
    }

    [Test]
    public void Select_WithCompoundQuery_Except()
    {
        var all = _db
            .Select(UsersTable.Id)
            .From(users);

        var members = _db
            .Select(UserProjectsTable.UserId)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.IsActive, true));

        var compound = all.Except(members);

        var (sql, parameters) = compound.Build();
        Print("PgSQL EXCEPT compound query", sql, parameters);
    }

    // =========================================================================
    // PgSQL: Extended Lock Types
    // =========================================================================

    [Test]
    public void Select_ForNoKeyUpdateSkipLocked()
    {
        var query = _db.Select(PgUserSelect.Record)
            .From(users)
            .ForNoKeyUpdate(skipLocked: true, nowait: false, UsersTable.Id, UsersTable.Name);

        var (sql, parameters) = query.Build();
        Print("PgSQL FOR NO KEY UPDATE SKIP LOCKED OF", sql, parameters);
    }


    // =========================================================================
    // PgSQL: Array Operators (ArrayAny, ArrayAll)
    // =========================================================================

    [Test]
    public void Select_PgArrayAny()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.Any(UsersTable.Id, Sql.Raw<long>("(SELECT user_id FROM user_projects)")));

        var (sql, parameters) = query.Build();
        Print("PgSQL ANY array operator", sql, parameters);
    }

    [Test]
    public void Select_PgArrayAll()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(PgOperators.All(UsersTable.Id, Sql.Raw<long>("(SELECT user_id FROM user_projects)")));

        var (sql, parameters) = query.Build();
        Print("PgSQL ALL array operator", sql, parameters);
    }

    // =========================================================================
    // PgSQL: Not() and IIf()
    // =========================================================================

    [Test]
    public void Select_NotOperator()
    {
        var query = _db
            .Select(PgUserSelect.Record)
            .From(users)
            .Where(Not(Eq(UsersTable.IsActive, true)));

        var (sql, parameters) = query.Build();
        Print("PgSQL NOT operator", sql, parameters);
    }

    [Test]
    public void Select_IiFunction()
    {
        var query = _db
            .Select(
                UsersTable.Name,
                IIf<string>(Gt(UsersTable.Age, 18), Sql.Value("Adult"), Sql.Value("Minor")).As("Status")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("PgSQL IIF function", sql, parameters);
    }
}

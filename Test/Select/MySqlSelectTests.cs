using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.MySql;
using SharedDemo.MySql;
using static Drizzle4Dotnet.Core.Operators.Operators;
using static Drizzle4Dotnet.Core.Operators.Functions;
using MySqlUserSelect = SharedDemo.MySql.UserSelect;
using MySqlUserWithRelationsSelect = SharedDemo.MySql.UserWithRelationsSelect;
using MySqlProjectSelect = SharedDemo.MySql.ProjectSelect;

namespace Test.Select;

[TestFixture]
public class MySqlSelectTests
{
    private MySqlQueryBuilder _db = null!;
    private UsersTable users = null!;
    private DepartmentsTable departments = null!;
    private RolesTable roles = null!;
    private ManagersTable managers = null!;
    private ProjectsTable projects = null!;
    private UserProjectsTable userProjects = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MySqlQueryBuilder();

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

    // ===================== BASIC SELECT =====================

    [Test]
    public void Select_Basic()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL Basic SELECT", sql, parameters);
    }

    [Test]
    public void Select_WithWhere()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(And(
                UsersTable.Id.Eq(1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithJoin()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(departments,
                Eq(UsersTable.DepartmentId, DepartmentsTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with JOIN", sql, parameters);
    }

    [Test]
    public void Select_WithMultipleJoins()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with MULTIPLE JOINS", sql, parameters);
    }

    [Test]
    public void Select_SelfJoin_Alias()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL SELF JOIN (Alias)", sql, parameters);
    }

    [Test]
    public void Select_ManyToMany()
    {
        var query = _db
            .Select(MySqlProjectSelect.Record)
            .From(userProjects)
            .InnerJoin(projects, Eq(UserProjectsTable.ProjectId, ProjectsTable.Id))
            .InnerJoin(users, Eq(UserProjectsTable.UserId, UsersTable.Id));

        var (sql, parameters) = query.Build();
        Print("MySQL MANY-TO-MANY JOIN", sql, parameters);
    }

    [Test]
    public void Select_ComplexWhere()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(
                Eq(UsersTable.IsActive, true),
                Eq(UsersTable.Age, 30),
                Like(UsersTable.Email, "%@company.com")
            );

        var (sql, parameters) = query.Build();
        Print("MySQL COMPLEX WHERE", sql, parameters);
    }

    [Test]
    public void Select_NullCheck()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(IsNull(UsersTable.ManagerId));

        var (sql, parameters) = query.Build();
        Print("MySQL NULL CHECK", sql, parameters);
    }

    [Test]
    public void Select_FullComplex()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .InnerJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .Where(And(
                Eq(DepartmentsTable.Id, 1),
                Like(UsersTable.Email, "%@example.com")
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL FULL COMPLEX QUERY", sql, parameters);
    }

    // ===================== ORDER BY / LIMIT / OFFSET =====================

    [Test]
    public void Select_WithOrderBy()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Name)
            .OrderBy(UsersTable.Age, false);

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with ORDER BY", sql, parameters);
    }

    [Test]
    public void Select_WithLimitOffset()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .OrderBy(UsersTable.Id)
            .Limit(10)
            .Offset(20);

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with LIMIT/OFFSET", sql, parameters);
    }

    // ===================== AGGREGATE / GROUP BY / HAVING =====================

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
        Print("MySQL SELECT with GROUP BY and HAVING", sql, parameters);
    }

    [Test]
    public void Select_WithDistinct()
    {
        var query = _db
            .SelectDistinct(MySqlUserSelect.Record)
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT DISTINCT", sql, parameters);
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
                CountDistinct(UsersTable.Email).As("UniqueEmails")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with AGGREGATE functions", sql, parameters);
    }

    // ===================== SUBQUERIES =====================

    [Test]
    public void Select_WithSubquery()
    {
        var subQuery = _db
            .Select(UsersTable.Id)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true));

        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with SUBQUERY", sql, parameters);
    }

    [Test]
    public void Select_WithExists()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 1));

        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Exists(subQuery));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with EXISTS", sql, parameters);
    }

    [Test]
    public void Select_WithInSubquery()
    {
        var subQuery = _db
            .Select(UserProjectsTable.Id)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.ProjectId, 2));

        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, subQuery));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with IN SUBQUERY", sql, parameters);
    }

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
        Print("MySQL SELECT FROM SUBQUERY (Derived Table)", sql, parameters);
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
        Print("MySQL SELECT with SUBQUERY in SELECT list", sql, parameters);
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
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, outerSub));

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT with NESTED SUBQUERIES", sql, parameters);
    }

    [Test]
    public void Select_ComplexExistsAndJoins()
    {
        var subQuery = _db
            .Select(UserProjectsTable.ModelAll)
            .From(userProjects)
            .Where(Eq(UserProjectsTable.UserId, UsersTable.Id));

        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .Where(And(
                Exists(subQuery),
                Eq(DepartmentsTable.Name, "Engineering")
            ))
            .OrderBy(UsersTable.Name);

        var (sql, parameters) = query.Build();
        Print("MySQL COMPLEX SELECT with EXISTS and JOIN", sql, parameters);
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
        Print("MySQL SELECT with AGGREGATED SUBQUERY", sql, parameters);
    }

    // ===================== CTE =====================

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
        Print("MySQL CTE", sql, parameters);
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
        Print("MySQL CTE Salary Gap", sql, parameters);
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
        Print("MySQL CTE as Filter Scope", sql, parameters);
    }

    // ===================== CASE EXPRESSIONS =====================

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
        Print("MySQL SELECT with CASE expression", sql, parameters);
    }

    // ===================== OPERATORS =====================

    [Test]
    public void Select_ComparisonOperators()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
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
        Print("MySQL Comparison Operators", sql, parameters);
    }

    [Test]
    public void Select_LogicalOperators()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(And(
                Or(
                    Eq(UsersTable.IsActive, true),
                    Eq(UsersTable.IsActive, false)
                ),
                Not(Eq(UsersTable.Name, "Test"))
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL Logical Operators (AND, OR, NOT)", sql, parameters);
    }

    [Test]
    public void Select_StringOperators()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(And(
                Like(UsersTable.Email, "%@example.com"),
                NotLike(UsersTable.Email, "test%@"),
                Contains(UsersTable.Name, "John"),
                StartsWith(UsersTable.Name, "J"),
                EndsWith(UsersTable.Email, ".com")
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL String Operators (LIKE, Contains, etc)", sql, parameters);
    }

    [Test]
    public void Select_InOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(In(UsersTable.Id, [1, 2, 3, 4]));

        var (sql, parameters) = query.Build();
        Print("MySQL IN Operator with list", sql, parameters);
    }

    [Test]
    public void Select_NotInOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(NotIn(UsersTable.Id, [1L, 2L, 3L]));

        var (sql, parameters) = query.Build();
        Print("MySQL NOT IN Operator", sql, parameters);
    }

    [Test]
    public void Select_BetweenOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Between(UsersTable.Age, 18, 65));

        var (sql, parameters) = query.Build();
        Print("MySQL BETWEEN Operator", sql, parameters);
    }

    [Test]
    public void Select_NotBetweenOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(NotBetween(UsersTable.Age, 18, 25));

        var (sql, parameters) = query.Build();
        Print("MySQL NOT BETWEEN Operator", sql, parameters);
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
        Print("MySQL Arithmetic Operators", sql, parameters);
    }

    [Test]
    public void Select_ArithmeticOnColumn()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Gt(
                Add(UsersTable.Salary, 5000m),
                100000m
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL Arithmetic with Add in WHERE", sql, parameters);
    }

    // ===================== MYSQL-SPECIFIC OPERATORS =====================

    [Test]
    public void Select_MySqlNullSafeEqual()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(MySqlOperators.NullSafeEqual(UsersTable.ManagerId, null));

        var (sql, parameters) = query.Build();
        Print("MySQL NullSafeEqual (<=>)", sql, parameters);
    }

    [Test]
    public void Select_MySqlRegexp()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(MySqlOperators.Regexp(UsersTable.Email, "^[a-z]+@example\\.com$"));

        var (sql, parameters) = query.Build();
        Print("MySQL REGEXP", sql, parameters);
    }

    [Test]
    public void Select_MySqlNotRegexp()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(MySqlOperators.NotRegexp(UsersTable.Email, "^test.*"));

        var (sql, parameters) = query.Build();
        Print("MySQL NOT REGEXP", sql, parameters);
    }

    // ===================== MYSQL-SPECIFIC FUNCTIONS =====================

    [Test]
    public void Select_MySqlConcat()
    {
        var query = _db
            .Select(
                MySqlFunctions.Concat(UsersTable.Name, Sql.Value(" - "), UsersTable.Email).As("NameEmail")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL CONCAT", sql, parameters);
    }

    [Test]
    public void Select_MySqlConcatWs()
    {
        var query = _db
            .Select(
                MySqlFunctions.ConcatWs(", ", UsersTable.Id, UsersTable.Name, UsersTable.Email).As("Info")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL CONCAT_WS", sql, parameters);
    }

    [Test]
    public void Select_MySqlCharLength()
    {
        var query = _db
            .Select(
                MySqlFunctions.CharLength(UsersTable.Name).As("NameLength")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL CHAR_LENGTH", sql, parameters);
    }

    [Test]
    public void Select_MySqlLocate()
    {
        var query = _db
            .Select(
                MySqlFunctions.Locate(Sql.Value("@"), UsersTable.Email).As("AtPos")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL LOCATE", sql, parameters);
    }

    [Test]
    public void Select_MySqlLocateWithPosition()
    {
        var query = _db
            .Select(
                MySqlFunctions.Locate(Sql.Value("a"), UsersTable.Email, 3).As("AtPos")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL LOCATE with start pos", sql, parameters);
    }

    [Test]
    public void Select_MySqlSubstringIndex()
    {
        var query = _db
            .Select(
                MySqlFunctions.SubstringIndex(UsersTable.Email, Sql.Value("@"), 1).As("Username")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL SUBSTRING_INDEX", sql, parameters);
    }

    [Test]
    public void Select_MySqlPosition()
    {
        var query = _db
            .Select(
                MySqlFunctions.Position(UsersTable.Email, "@").As("AtPos")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL POSITION", sql, parameters);
    }

    [Test]
    public void Select_MySqlDateAdd()
    {
        var query = _db
            .Select(
                MySqlFunctions.DateAdd(UsersTable.CreatedAt, 7, "DAY").As("Plus7Days")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL DATE_ADD", sql, parameters);
    }

    [Test]
    public void Select_MySqlDateSub()
    {
        var query = _db
            .Select(
                MySqlFunctions.DateSub(UsersTable.CreatedAt, 30, "DAY").As("Minus30Days")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL DATE_SUB", sql, parameters);
    }

    [Test]
    public void Select_MySqlDateFormat()
    {
        var query = _db
            .Select(
                MySqlFunctions.DateFormat(UsersTable.CreatedAt, "%Y-%m-%d").As("FormattedDate")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL DATE_FORMAT", sql, parameters);
    }

    [Test]
    public void Select_MySqlUnixTimestamp()
    {
        var query = _db
            .Select(
                MySqlFunctions.UnixTimestamp(UsersTable.CreatedAt).As("UnixTs")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL UNIX_TIMESTAMP", sql, parameters);
    }

    [Test]
    public void Select_MySqlFromUnixTime()
    {
        var query = _db
            .Select(
                MySqlFunctions.FromUnixTime(UsersTable.Id).As("FromUnix")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL FROM_UNIXTIME", sql, parameters);
    }

    [Test]
    public void Select_MySqlStrToDate()
    {
        var query = _db
            .Select(
                MySqlFunctions.StrToDate(Sql.Value("2024-01-15"), "%Y-%m-%d").As("ParsedDate")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL STR_TO_DATE", sql, parameters);
    }

    [Test]
    public void Select_MySqlRand()
    {
        var query = _db
            .Select(
                UsersTable.Id,
                MySqlFunctions.Rand<double>().As("Random")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL RAND", sql, parameters);
    }

    [Test]
    public void Select_MySqlTruncate()
    {
        var query = _db
            .Select(
                MySqlFunctions.Truncate(UsersTable.Rating, 1).As("TruncRating")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL TRUNCATE", sql, parameters);
    }

    [Test]
    public void Select_MySqlBitCount()
    {
        var query = _db
            .Select(
                MySqlFunctions.BitCount(UsersTable.Id).As("Bits")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL BIT_COUNT", sql, parameters);
    }

    [Test]
    public void Select_MySqlCrc32()
    {
        var query = _db
            .Select(
                MySqlFunctions.Crc32(UsersTable.Email).As("Crc")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL CRC32", sql, parameters);
    }

    [Test]
    public void Select_MySqlCurDate()
    {
        var query = _db
            .Select(MySqlFunctions.CurDate().As("Today"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL CURDATE", sql, parameters);
    }

    [Test]
    public void Select_MySqlCurTime()
    {
        var query = _db
            .Select(MySqlFunctions.CurTime().As("Now"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL CURTIME", sql, parameters);
    }

    [Test]
    public void Select_MySqlLastInsertId()
    {
        var query = _db
            .Select(MySqlFunctions.LastInsertId().As("LastId"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL LAST_INSERT_ID", sql, parameters);
    }

    [Test]
    public void Select_MySqlDatabase()
    {
        var query = _db
            .Select(MySqlFunctions.Database().As("DbName"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL DATABASE()", sql, parameters);
    }

    [Test]
    public void Select_MySqlVersion()
    {
        var query = _db
            .Select(MySqlFunctions.Version().As("Ver"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL VERSION()", sql, parameters);
    }

    [Test]
    public void Select_MySqlUser()
    {
        var query = _db
            .Select(MySqlFunctions.User().As("User"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL USER()", sql, parameters);
    }

    // ===================== MYSQL JSON FUNCTIONS =====================

    [Test]
    public void Select_MySqlJsonExtract()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonExtract<long, string>(UsersTable.Id, "$.key").As("Val")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_EXTRACT", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonExtractText()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonExtractText(UsersTable.Id, "$.name").As("Name")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_UNQUOTE(JSON_EXTRACT)", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonArrow()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonArrow<long, string>(UsersTable.Id, "$.key").As("Val")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON ->", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonArrowText()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonArrowText(UsersTable.Id, "$.name").As("Name")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON ->>", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonArrayAgg()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonArrayAgg(UsersTable.Name).As("Names")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_ARRAYAGG", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonObjectAgg()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonObjectAgg(Sql.Value("name"), UsersTable.Name).As("Obj")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_OBJECTAGG", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonArray()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonArray(UsersTable.Id, UsersTable.Name).As("Arr")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_ARRAY", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonObject()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonObject(Sql.Value("id"), UsersTable.Id, Sql.Value("name"), UsersTable.Name).As("Obj")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_OBJECT", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonContains()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(MySqlFunctions.JsonContains(UsersTable.Id, Sql.Value("\"test\"")));

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_CONTAINS", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonLength()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonLength(UsersTable.Id).As("Len")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_LENGTH", sql, parameters);
    }

    [Test]
    public void Select_MySqlJsonKeys()
    {
        var query = _db
            .Select(
                MySqlFunctions.JsonKeys(UsersTable.Id).As("Keys")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL JSON_KEYS", sql, parameters);
    }

    // ===================== STANDARD FUNCTIONS (MySql) =====================

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
        Print("MySQL Standard String Functions", sql, parameters);
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
        Print("MySQL Standard Numeric Functions", sql, parameters);
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
        Print("MySQL Standard DateTime Functions", sql, parameters);
    }

    [Test]
    public void Select_ConditionalFunctions()
    {
        var query = _db
            .Select(
                Coalesce(UsersTable.ManagerId, 0).As("ManagerIdOrDefault"),
                NullIf(UsersTable.Name, "Admin").As("NameOrNull"),
                IIf<string>(Eq(UsersTable.IsActive, true), Sql.Value("Active"), Sql.Value("Inactive")).As("Status")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL Conditional Functions", sql, parameters);
    }

    // ===================== CAST / TYPE CONVERSION =====================

    [Test]
    public void Select_CastFunctions()
    {
        var query = _db
            .Select(
                Cast<string>(UsersTable.Id, "CHAR").As("IdStr"),
                CastToString(UsersTable.Age).As("AgeStr"),
                CastToInt(UsersTable.Rating).As("RatingInt"),
                CastToDouble(UsersTable.Salary).As("SalaryDbl"),
                CastToDateTime(UsersTable.Id).As("DateTimeCast")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL CAST Functions", sql, parameters);
    }

    // ===================== ADVANCED QUERIES =====================

    [Test]
    public void Select_ComplexJoinsAndWhere()
    {
        var query = _db
            .Select(MySqlUserWithRelationsSelect.Record)
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
        Print("MySQL COMPLEX JOIN + WHERE + ORDER + LIMIT", sql, parameters);
    }

    [Test]
    public void Select_AllJoinTypes()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
            .LeftJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id))
            .RightJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
            .CrossJoin(projects);

        var (sql, parameters) = query.Build();
        Print("MySQL All Join Types", sql, parameters);
    }

    [Test]
    public void Select_WithForUpdate()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForUpdate();

        var (sql, parameters) = query.Build();
        Print("MySQL FOR UPDATE", sql, parameters);
    }

    [Test]
    public void Select_WithForShare()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Eq(UsersTable.Id, 1))
            .ForShare();

        var (sql, parameters) = query.Build();
        Print("MySQL FOR SHARE", sql, parameters);
    }

    [Test]
    public void Select_WithRawSQL()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Sql.Raw("`Age` > 18"));

        var (sql, parameters) = query.Build();
        Print("MySQL Raw SQL WHERE", sql, parameters);
    }

    [Test]
    public void Select_WithInValuesList()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(In<long, MySqlSqlDialectImpl>(UsersTable.Id, 1L, 2L, 3L, 4L, 5L));

        var (sql, parameters) = query.Build();
        Print("MySQL IN with values list", sql, parameters);
    }

    [Test]
    public void Select_WithMultipleCTEs()
    {
        var activeUsers = _db
            .Select(UsersTable.ModelAll)
            .From(users)
            .Where(Eq(UsersTable.IsActive, true))
            .AsSubQuery("active_users").AsCte();

        var projectMembers = _db
            .Select(
                UserProjectsTable.UserId,
                ProjectsTable.Name)
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
        Print("MySQL Multiple CTEs Join", sql, parameters);
    }

    [Test]
    public void Select_WithXorOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Xor(
                Eq(UsersTable.IsActive, true),
                Gt(UsersTable.Rating, 4.0)
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL XOR Operator", sql, parameters);
    }

    [Test]
    public void Select_MySqlRowCount()
    {
        var query = _db
            .Select(MySqlFunctions.RowCount().As("Rows"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL ROW_COUNT", sql, parameters);
    }

    [Test]
    public void Select_MySqlFoundRows()
    {
        var query = _db
            .Select(MySqlFunctions.FoundRows().As("Total"))
            .From(users).Limit(1);

        var (sql, parameters) = query.Build();
        Print("MySQL FOUND_ROWS", sql, parameters);
    }

    [Test]
    public void Select_ProjectBudgetVsDeptTotal_CTE()
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
        Print("MySQL CTE with Aggregation", sql, parameters);
    }

    // =========================================================================
    // MySQL: Missing SELECT query features
    // =========================================================================


    [Test]
    public void Select_WithSubqueryInFrom()
    {
        var subquery = _db
            .Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Where(Gt(UsersTable.Age, 18))
            .AsSubQuery("active_users", (from) => new
            {
                Id = from.Field<long>("Id"),
                Name = from.Field<string>("Name")
            });

        var query = _db
            .Select(subquery.Shape.Id, subquery.Shape.Name)
            .From(subquery);

        var (sql, parameters) = query.Build();
        Print("MySQL SELECT from subquery (derived table)", sql, parameters);
    }

    [Test]
    public void Select_IsNull_IsNotNull()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(And(
                IsNull(UsersTable.DeletedAt),
                IsNotNull(UsersTable.Email)
            ));

        var (sql, parameters) = query.Build();
        Print("MySQL IS NULL / IS NOT NULL", sql, parameters);
    }

    [Test]
    public void Select_NotOperator()
    {
        var query = _db
            .Select(MySqlUserSelect.Record)
            .From(users)
            .Where(Not(Eq(UsersTable.IsActive, true)));

        var (sql, parameters) = query.Build();
        Print("MySQL NOT operator", sql, parameters);
    }

    // =========================================================================
    // MySQL: Standard functions (IIf, stats aggregate)
    // =========================================================================

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
        Print("MySQL IIF function", sql, parameters);
    }

    [Test]
    public void Select_WithStdDevAndVariance()
    {
        var query = _db
            .Select(
                StdDev(UsersTable.Salary).As("StdSalary"),
                Variance(UsersTable.Salary).As("VarSalary"),
                VarPop(UsersTable.Salary).As("VarPopSalary"),
                StdDevPop(UsersTable.Salary).As("StdPopSalary")
            )
            .From(users);

        var (sql, parameters) = query.Build();
        Print("MySQL STDDEV and VARIANCE", sql, parameters);
    }

    // =========================================================================
    // MySQL CTE (WITH)
    // =========================================================================

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
        Print("MySQL SELECT WITH CTE", sql, parameters);
    }

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
        Print("MySQL SELECT WITH RECURSIVE CTE", sql, parameters);
    }
}

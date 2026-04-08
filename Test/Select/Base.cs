using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Test;

using static Drizzle4Dotnet.Core.Shared.Operators.Operators;

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
                Eq(UsersTable.Id, 1),
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
    
    
}

[DbSelect]
public partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get;set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set;}
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set;}
}


[DbSelect]
public partial class UserWithRelationsSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int UserId { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string UserName { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set; }

    [MapWith(typeof(DepartmentsTable), DepartmentsTable.ColumnNames.Name)]
    public string DepartmentName { get; set; }

    [MapWith(typeof(RolesTable), RolesTable.ColumnNames.Name)]
    public string RoleName { get; set; }

    [MapWith(typeof(ManagersTable), ManagersTable.ColumnNames.Name)]
    public string ManagerName { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.IsActive)]
    public bool IsActive { get; set; }
}

[DbSelect]
public partial class ProjectSelect
{
    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Id)]
    public int ProjectId { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Name)]
    public string ProjectName { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Code)]
    public string Code { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Budget)]
    public decimal Budget { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string OwnerName { get; set; }

    [MapWith(typeof(DepartmentsTable), DepartmentsTable.ColumnNames.Name)]
    public string DepartmentName { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.IsActive)]
    public bool IsActive { get; set; }
}

[DbSelect]
public partial class UserProjectSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int UserId { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string UserName { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Id)]
    public int ProjectId { get; set; }

    [MapWith(typeof(ProjectsTable), ProjectsTable.ColumnNames.Name)]
    public string ProjectName { get; set; }

    [MapWith(typeof(UserProjectsTable), UserProjectsTable.ColumnNames.Role)]
    public string Role { get; set; }

    [MapWith(typeof(UserProjectsTable), UserProjectsTable.ColumnNames.Allocation)]
    public double Allocation { get; set; }

    [MapWith(typeof(UserProjectsTable), UserProjectsTable.ColumnNames.AssignedAt)]
    public DateTime AssignedAt { get; set; }
}

[DbSelect]
public partial class UserFullSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Guid)]
    public Guid Guid { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Age)]
    public int Age { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Salary)]
    public decimal Salary { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Rating)]
    public double Rating { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.IsActive)]
    public bool IsActive { get; set; }

    [MapWith(typeof(DepartmentsTable), DepartmentsTable.ColumnNames.Name)]
    public string DepartmentName { get; set; }

    [MapWith(typeof(RolesTable), RolesTable.ColumnNames.Name)]
    public string RoleName { get; set; }

    [MapWith(typeof(ManagersTable), ManagersTable.ColumnNames.Name)]
    public string ManagerName { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.CreatedAt)]
    public DateTime CreatedAt { get; set; }
}
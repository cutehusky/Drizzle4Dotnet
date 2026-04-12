using Drizzle4Dotnet.Core.Shared;

namespace SharedDemo;


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
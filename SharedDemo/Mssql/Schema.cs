using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Mssql;
using Drizzle4Dotnet.Mssql.Schema;

namespace SharedDemo.Mssql
{
    [Table("Users", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Users_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Roles", new[] { "RoleId" }, typeof(RolesTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [MssqlBigInt]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [MssqlUniqueIdentifier]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [MssqlNVarChar(255)]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [MssqlNVarChar(255)]
            [Column("Email",NotNull = true)]
            public static string Email { get; set; }

            [MssqlBigInt]
            [Column("Age",NotNull = true)]
            public static long Age { get; set; }

            [MssqlDecimal]
            [Column("Salary",DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [MssqlFloat]
            [Column("Rating",NotNull = true)]
            public static double Rating { get; set; }

            [MssqlBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [MssqlBigInt]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [MssqlBigInt]
            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [MssqlBigInt]
            [Column("RoleId",NotNull = true)]
            public static long RoleId { get; set; }

            [MssqlDateTime2]
            [Column("CreatedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [MssqlDateTime2]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager")]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Departments_Parent", new[] { "ParentDepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Departments_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [MssqlBigInt]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [MssqlUniqueIdentifier]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [MssqlNVarChar(255)]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [MssqlNVarChar(100)]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [MssqlNVarChar(255)]
            [Column("Location",NotNull = true)]
            public static string Location { get; set; }

            [MssqlDecimal]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [MssqlBigInt]
            [Column("HeadCount",DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [MssqlBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [MssqlDateTime2]
            [Column("CreatedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [MssqlDateTime2]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [MssqlNVarChar]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
            
            [MssqlBigInt]
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [MssqlBigInt]
            [Column("ManagerId",NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [MssqlBigInt]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [MssqlUniqueIdentifier]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [MssqlNVarChar(100)]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [MssqlBigInt]
            [Column("Level",NotNull = true)]
            public static long Level { get; set; }

            [MssqlDecimal]
            [Column("BaseSalary",DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [MssqlFloat]
            [Column("BonusRate",DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [MssqlBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [MssqlBoolean]
            [Column("CanApproveBudget",NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [MssqlDateTime2]
            [Column("CreatedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [MssqlDateTime2]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [MssqlNVarChar]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Projects_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Projects_Owner", new[] { "OwnerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [MssqlBigInt]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [MssqlUniqueIdentifier]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [MssqlNVarChar(255)]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [MssqlNVarChar(100)]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [MssqlBigInt]
            [Column("OwnerId",NotNull = true)]
            public static long OwnerId { get; set; }

            [MssqlBigInt]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [MssqlDecimal]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [MssqlFloat]
            [Column("Progress",DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [MssqlBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [MssqlDateTime2]
            [Column("StartDate",NotNull = true)]
            public static DateTime StartDate { get; set; }

            [MssqlDateTime2]
            [Column("EndDate")]
            public static DateTime? EndDate { get; set; }

            [MssqlDateTime2]
            [Column("CreatedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [MssqlDateTime2]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_UserProjects_User", new[] { "UserId" }, typeof(UsersTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_UserProjects_Project", new[] { "ProjectId" }, typeof(ProjectsTable), new[] { "Id" })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [MssqlBigInt]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [MssqlBigInt]
            [Column("UserId",NotNull = true)]
            public static long UserId { get; set; }

            [MssqlBigInt]
            [Column("ProjectId",NotNull = true)]
            public static long ProjectId { get; set; }

            [MssqlNVarChar(100)]
            [Column("Role",NotNull = true)]
            public static string Role { get; set; }

            [MssqlFloat]
            [Column("Allocation",DefaultValue = "0", NotNull = true)]
            public static double Allocation { get; set; }

            [MssqlDecimal]
            [Column("HourlyRate",DefaultValue = "0", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [MssqlBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [MssqlDateTime2]
            [Column("AssignedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime AssignedAt { get; set; }

            [MssqlDateTime2]
            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [MssqlDateTime2]
            [Column("CreatedAt",NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [MssqlDateTime2]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Oracle;
using Drizzle4Dotnet.Oracle.Schema;

namespace SharedDemo.Oracle
{
    [Table("Users", null, Dialect = typeof(OracleSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Users_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Roles", new[] { "RoleId" }, typeof(RolesTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [OracleInteger]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [OracleUuid]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [OracleVarChar2]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [OracleVarChar2]
            [Column("Email",NotNull = true)]
            public static string Email { get; set; }

            [OracleInteger]
            [Column("Age",NotNull = true)]
            public static long Age { get; set; }

            [OracleDecimal]
            [Column("Salary",DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [OracleBinaryDouble]
            [Column("Rating",NotNull = true)]
            public static double Rating { get; set; }

            [OracleBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [OracleInteger]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [OracleInteger]
            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [OracleInteger]
            [Column("RoleId",NotNull = true)]
            public static long RoleId { get; set; }

            [OracleTimestamp]
            [Column("CreatedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [OracleTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager")]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", null, Dialect = typeof(OracleSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Departments_Parent", new[] { "ParentDepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Departments_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [OracleInteger]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [OracleUuid]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [OracleVarChar2]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [OracleVarChar2]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [OracleVarChar2]
            [Column("Location",NotNull = true)]
            public static string Location { get; set; }

            [OracleDecimal]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [OracleInteger]
            [Column("HeadCount",DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [OracleBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [OracleTimestamp]
            [Column("CreatedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [OracleTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [OracleText]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
            
            [OracleInteger]
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [OracleInteger]
            [Column("ManagerId",NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", null, Dialect = typeof(OracleSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [OracleInteger]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [OracleUuid]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [OracleVarChar2]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [OracleInteger]
            [Column("Level",NotNull = true)]
            public static long Level { get; set; }

            [OracleDecimal]
            [Column("BaseSalary",DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [OracleBinaryDouble]
            [Column("BonusRate",DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [OracleBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [OracleBoolean]
            [Column("CanApproveBudget",NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [OracleTimestamp]
            [Column("CreatedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [OracleTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [OracleText]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", null, Dialect = typeof(OracleSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Projects_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Projects_Owner", new[] { "OwnerId" }, typeof(UsersTable), new[] { "Id" })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [OracleInteger]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [OracleUuid]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [OracleVarChar2]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [OracleVarChar2]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [OracleInteger]
            [Column("OwnerId",NotNull = true)]
            public static long OwnerId { get; set; }

            [OracleInteger]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [OracleDecimal]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [OracleBinaryDouble]
            [Column("Progress",DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [OracleBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [OracleDate]
            [Column("StartDate",NotNull = true)]
            public static DateTime StartDate { get; set; }

            [OracleDate]
            [Column("EndDate")]
            public static DateTime? EndDate { get; set; }

            [OracleTimestamp]
            [Column("CreatedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [OracleTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", null, Dialect = typeof(OracleSqlDialectImpl))]
    [ForeignKeyConstraint("FK_UserProjects_User", new[] { "UserId" }, typeof(UsersTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_UserProjects_Project", new[] { "ProjectId" }, typeof(ProjectsTable), new[] { "Id" })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [OracleInteger]
            [Column("Id",PrimaryKey = true)]
            public static long Id { get; set; }

            [OracleInteger]
            [Column("UserId",NotNull = true)]
            public static long UserId { get; set; }

            [OracleInteger]
            [Column("ProjectId",NotNull = true)]
            public static long ProjectId { get; set; }

            [OracleVarChar2]
            [Column("Role",NotNull = true)]
            public static string Role { get; set; }

            [OracleBinaryDouble]
            [Column("Allocation",DefaultValue = "0", NotNull = true)]
            public static double Allocation { get; set; }

            [OracleDecimal]
            [Column("HourlyRate",DefaultValue = "0", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [OracleBoolean]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [OracleTimestamp]
            [Column("AssignedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime AssignedAt { get; set; }

            [OracleTimestamp]
            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [OracleTimestamp]
            [Column("CreatedAt",NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [OracleTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

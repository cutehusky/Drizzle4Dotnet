using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace SharedDemo.MySql
{
    [Table("Users", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class UsersTable
    {
        public static class Columns
        {
            [Column("Id")]
            public static long Id { get; set; }

            [Column("Guid")]
            public static Guid Guid { get; set; }

            [Column("Name")]
            public static string Name { get; set; }

            [Column("Email")]
            public static string Email { get; set; }

            [Column("Age")]
            public static long Age { get; set; }

            [Column("Salary")]
            public static decimal Salary { get; set; }

            [Column("Rating")]
            public static double Rating { get; set; }

            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [Column("DepartmentId")]
            public static long DepartmentId { get; set; }

            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [Column("RoleId")]
            public static long RoleId { get; set; }

            [Column("CreatedAt")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
            
            [Column("DeletedAt")]
            public static DateTime? DeletedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [Column("Id")]
            public static long Id { get; set; }

            [Column("Guid")]
            public static Guid Guid { get; set; }

            [Column("Name")]
            public static string Name { get; set; }

            [Column("Code")]
            public static string Code { get; set; }

            [Column("Location")]
            public static string Location { get; set; }

            [Column("Budget")]
            public static decimal Budget { get; set; }

            [Column("HeadCount")]
            public static long HeadCount { get; set; }

            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [Column("CreatedAt")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description")]
            public static string Description { get; set; }
            
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [Column("ManagerId")]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [Column("Id")]
            public static long Id { get; set; }

            [Column("Guid")]
            public static Guid Guid { get; set; }

            [Column("Name")]
            public static string Name { get; set; }

            [Column("Level")]
            public static long Level { get; set; }

            [Column("BaseSalary")]
            public static decimal BaseSalary { get; set; }

            [Column("BonusRate")]
            public static double BonusRate { get; set; }

            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [Column("CanApproveBudget")]
            public static bool CanApproveBudget { get; set; }

            [Column("CreatedAt")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description")]
            public static string Description { get; set; }
        }
    }

    [Table("Projects", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [Column("Id")] public static long Id { get; set; }

            [Column("Guid")] public static Guid Guid { get; set; }

            [Column("Name")] public static string Name { get; set; }

            [Column("Code")] public static string Code { get; set; }

            [Column("OwnerId")] public static long OwnerId { get; set; }

            [Column("DepartmentId")] public static long DepartmentId { get; set; }

            [Column("Budget")] public static decimal Budget { get; set; }

            [Column("Progress")] public static double Progress { get; set; }

            [Column("IsActive")] public static bool IsActive { get; set; }

            [Column("StartDate")] public static DateTime StartDate { get; set; }

            [Column("EndDate")] public static DateTime? EndDate { get; set; }

            [Column("CreatedAt")] public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt")] public static DateTime? UpdatedAt { get; set; }
        }
    }

    [Table("UserProjects", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [Column("Id")]
            public static long Id { get; set; }

            [Column("UserId")]
            public static long UserId { get; set; }

            [Column("ProjectId")]
            public static long ProjectId { get; set; }

            [Column("Role")]
            public static string Role { get; set; }

            [Column("Allocation")]
            public static double Allocation { get; set; }

            [Column("HourlyRate")]
            public static decimal HourlyRate { get; set; }

            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [Column("AssignedAt")]
            public static DateTime AssignedAt { get; set; }

            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [Column("CreatedAt")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

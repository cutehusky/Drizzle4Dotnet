using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace SharedDemo.Sqlite
{
    /// <summary>
    /// SQLite schema definitions mirroring the PgSql/MySql demos.
    /// Uses SQLite-compatible types: INTEGER, TEXT, REAL, NUMERIC, BLOB.
    /// SQLite has no schema support, so Schema is omitted from [Table] attributes.
    /// SQLite does not enforce FOREIGN KEY constraints by default,
    /// so constraints are documented for reference only.
    /// </summary>
    
    [Table("Users", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class UsersTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "INTEGER", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "TEXT", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "TEXT", NotNull = true)]
            public static string Name { get; set; }

            [Column("Email", DataType = "TEXT", NotNull = true)]
            public static string Email { get; set; }

            [Column("Age", DataType = "INTEGER", NotNull = true)]
            public static long Age { get; set; }

            [Column("Salary", DataType = "NUMERIC", DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [Column("Rating", DataType = "REAL", NotNull = true)]
            public static double Rating { get; set; }

            [Column("IsActive", DataType = "INTEGER", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("DepartmentId", DataType = "INTEGER", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("ManagerId", DataType = "INTEGER")]
            public static long? ManagerId { get; set; }

            [Column("RoleId", DataType = "INTEGER", NotNull = true)]
            public static long RoleId { get; set; }

            [Column("CreatedAt", DataType = "TEXT", NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TEXT")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("Departments", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "INTEGER", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "TEXT", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "TEXT", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "TEXT", NotNull = true)]
            public static string Code { get; set; }

            [Column("Location", DataType = "TEXT", NotNull = true)]
            public static string Location { get; set; }

            [Column("Budget", DataType = "NUMERIC", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("HeadCount", DataType = "INTEGER", DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [Column("IsActive", DataType = "INTEGER", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CreatedAt", DataType = "TEXT", NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TEXT")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "TEXT", NotNull = true)]
            public static string Description { get; set; }
            
            [Column("ParentDepartmentId", DataType = "INTEGER")]
            public static long? ParentDepartmentId { get; set; }
            
            [Column("ManagerId", DataType = "INTEGER", NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "INTEGER", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "TEXT", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "TEXT", NotNull = true)]
            public static string Name { get; set; }

            [Column("Level", DataType = "INTEGER", NotNull = true)]
            public static long Level { get; set; }

            [Column("BaseSalary", DataType = "NUMERIC", DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [Column("BonusRate", DataType = "REAL", DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [Column("IsActive", DataType = "INTEGER", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CanApproveBudget", DataType = "INTEGER", NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [Column("CreatedAt", DataType = "TEXT", NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TEXT")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "TEXT", NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "INTEGER", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "TEXT", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "TEXT", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "TEXT", NotNull = true)]
            public static string Code { get; set; }

            [Column("OwnerId", DataType = "INTEGER", NotNull = true)]
            public static long OwnerId { get; set; }

            [Column("DepartmentId", DataType = "INTEGER", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("Budget", DataType = "NUMERIC", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("Progress", DataType = "REAL", DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [Column("IsActive", DataType = "INTEGER", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("StartDate", DataType = "TEXT", NotNull = true)]
            public static DateTime StartDate { get; set; }

            [Column("EndDate", DataType = "TEXT")]
            public static DateTime? EndDate { get; set; }

            [Column("CreatedAt", DataType = "TEXT", NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TEXT")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "INTEGER", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [Column("UserId", DataType = "INTEGER", NotNull = true)]
            public static long UserId { get; set; }

            [Column("ProjectId", DataType = "INTEGER", NotNull = true)]
            public static long ProjectId { get; set; }

            [Column("Role", DataType = "TEXT", NotNull = true)]
            public static string Role { get; set; }

            [Column("Allocation", DataType = "REAL", NotNull = true)]
            public static double Allocation { get; set; }

            [Column("HourlyRate", DataType = "NUMERIC", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [Column("IsActive", DataType = "INTEGER", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("AssignedAt", DataType = "TEXT", NotNull = true)]
            public static DateTime AssignedAt { get; set; }

            [Column("RemovedAt", DataType = "TEXT")]
            public static DateTime? RemovedAt { get; set; }

            [Column("CreatedAt", DataType = "TEXT", NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TEXT")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

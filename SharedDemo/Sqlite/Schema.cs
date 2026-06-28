using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Sqlite;
using Drizzle4Dotnet.Sqlite.Schema;

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
            [SqliteInteger]
            [Column("Id",PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [SqliteText]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [SqliteText]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [SqliteText]
            [Column("Email",NotNull = true)]
            public static string Email { get; set; }

            [SqliteInteger]
            [Column("Age",NotNull = true)]
            public static long Age { get; set; }

            [SqliteNumeric]
            [Column("Salary",DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [SqliteReal]
            [Column("Rating",NotNull = true)]
            public static double Rating { get; set; }

            [SqliteInteger]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [SqliteInteger]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [SqliteInteger]
            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [SqliteInteger]
            [Column("RoleId",NotNull = true)]
            public static long RoleId { get; set; }

            [SqliteText]
            [Column("CreatedAt",NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [SqliteText]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("Departments", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [SqliteInteger]
            [Column("Id",PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [SqliteText]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [SqliteText]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [SqliteText]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [SqliteText]
            [Column("Location",NotNull = true)]
            public static string Location { get; set; }

            [SqliteNumeric]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [SqliteInteger]
            [Column("HeadCount",DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [SqliteInteger]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [SqliteText]
            [Column("CreatedAt",NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [SqliteText]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [SqliteText]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
            
            [SqliteInteger]
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [SqliteInteger]
            [Column("ManagerId",NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [SqliteInteger]
            [Column("Id",PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [SqliteText]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [SqliteText]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [SqliteInteger]
            [Column("Level",NotNull = true)]
            public static long Level { get; set; }

            [SqliteNumeric]
            [Column("BaseSalary",DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [SqliteReal]
            [Column("BonusRate",DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [SqliteInteger]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [SqliteInteger]
            [Column("CanApproveBudget",NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [SqliteText]
            [Column("CreatedAt",NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [SqliteText]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [SqliteText]
            [Column("Description",NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [SqliteInteger]
            [Column("Id",PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [SqliteText]
            [Column("Guid",NotNull = true)]
            public static Guid Guid { get; set; }

            [SqliteText]
            [Column("Name",NotNull = true)]
            public static string Name { get; set; }

            [SqliteText]
            [Column("Code",NotNull = true)]
            public static string Code { get; set; }

            [SqliteInteger]
            [Column("OwnerId",NotNull = true)]
            public static long OwnerId { get; set; }

            [SqliteInteger]
            [Column("DepartmentId",NotNull = true)]
            public static long DepartmentId { get; set; }

            [SqliteNumeric]
            [Column("Budget",DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [SqliteReal]
            [Column("Progress",DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [SqliteInteger]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [SqliteText]
            [Column("StartDate",NotNull = true)]
            public static DateTime StartDate { get; set; }

            [SqliteText]
            [Column("EndDate")]
            public static DateTime? EndDate { get; set; }

            [SqliteText]
            [Column("CreatedAt",NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [SqliteText]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", Dialect = typeof(SqliteSqlDialectImpl))]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [SqliteInteger]
            [Column("Id",PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [SqliteInteger]
            [Column("UserId",NotNull = true)]
            public static long UserId { get; set; }

            [SqliteInteger]
            [Column("ProjectId",NotNull = true)]
            public static long ProjectId { get; set; }

            [SqliteText]
            [Column("Role",NotNull = true)]
            public static string Role { get; set; }

            [SqliteReal]
            [Column("Allocation",NotNull = true)]
            public static double Allocation { get; set; }

            [SqliteNumeric]
            [Column("HourlyRate",NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [SqliteInteger]
            [Column("IsActive",NotNull = true)]
            public static bool IsActive { get; set; }

            [SqliteText]
            [Column("AssignedAt",NotNull = true)]
            public static DateTime AssignedAt { get; set; }

            [SqliteText]
            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [SqliteText]
            [Column("CreatedAt",NotNull = true, DefaultValue = "datetime('now')")]
            public static DateTime CreatedAt { get; set; }

            [SqliteText]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Mssql;

namespace SharedDemo.Mssql
{
    [Table("Users", "dbo", Dialect = typeof(MssqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT [FK_Users_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments]([Id])",
            "CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])",
            "CONSTRAINT [FK_Users_Manager] FOREIGN KEY ([ManagerId]) REFERENCES [Users]([Id])"
        })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "BIGINT", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "UNIQUEIDENTIFIER", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "NVARCHAR(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Email", DataType = "NVARCHAR(255)", NotNull = true)]
            public static string Email { get; set; }

            [Column("Age", DataType = "BIGINT", NotNull = true)]
            public static long Age { get; set; }

            [Column("Salary", DataType = "DECIMAL(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [Column("Rating", DataType = "FLOAT", NotNull = true)]
            public static double Rating { get; set; }

            [Column("IsActive", DataType = "BIT", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("DepartmentId", DataType = "BIGINT", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("ManagerId", DataType = "BIGINT")]
            public static long? ManagerId { get; set; }

            [Column("RoleId", DataType = "BIGINT", NotNull = true)]
            public static long RoleId { get; set; }

            [Column("CreatedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "DATETIME2")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager", Dialect = typeof(MssqlSqlDialectImpl))]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", "dbo", Dialect = typeof(MssqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT [FK_Departments_Parent] FOREIGN KEY ([ParentDepartmentId]) REFERENCES [Departments]([Id])",
            "CONSTRAINT [FK_Departments_Manager] FOREIGN KEY ([ManagerId]) REFERENCES [Users]([Id])"
        })]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "BIGINT", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "UNIQUEIDENTIFIER", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "NVARCHAR(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "NVARCHAR(100)", NotNull = true)]
            public static string Code { get; set; }

            [Column("Location", DataType = "NVARCHAR(255)", NotNull = true)]
            public static string Location { get; set; }

            [Column("Budget", DataType = "DECIMAL(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("HeadCount", DataType = "BIGINT", DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [Column("IsActive", DataType = "BIT", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CreatedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "DATETIME2")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "NVARCHAR(MAX)", NotNull = true)]
            public static string Description { get; set; }
            
            [Column("ParentDepartmentId", DataType = "BIGINT")]
            public static long? ParentDepartmentId { get; set; }
            
            [Column("ManagerId", DataType = "BIGINT", NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", "dbo", Dialect = typeof(MssqlSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "BIGINT", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "UNIQUEIDENTIFIER", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "NVARCHAR(100)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Level", DataType = "BIGINT", NotNull = true)]
            public static long Level { get; set; }

            [Column("BaseSalary", DataType = "DECIMAL(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [Column("BonusRate", DataType = "FLOAT", DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [Column("IsActive", DataType = "BIT", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CanApproveBudget", DataType = "BIT", NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [Column("CreatedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "DATETIME2")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "NVARCHAR(MAX)", NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", "dbo", Dialect = typeof(MssqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT [FK_Projects_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments]([Id])",
            "CONSTRAINT [FK_Projects_Owner] FOREIGN KEY ([OwnerId]) REFERENCES [Users]([Id])"
        })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "BIGINT", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "UNIQUEIDENTIFIER", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "NVARCHAR(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "NVARCHAR(100)", NotNull = true)]
            public static string Code { get; set; }

            [Column("OwnerId", DataType = "BIGINT", NotNull = true)]
            public static long OwnerId { get; set; }

            [Column("DepartmentId", DataType = "BIGINT", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("Budget", DataType = "DECIMAL(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("Progress", DataType = "FLOAT", DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [Column("IsActive", DataType = "BIT", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("StartDate", DataType = "DATETIME2", NotNull = true)]
            public static DateTime StartDate { get; set; }

            [Column("EndDate", DataType = "DATETIME2")]
            public static DateTime? EndDate { get; set; }

            [Column("CreatedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "DATETIME2")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", "dbo", Dialect = typeof(MssqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT [FK_UserProjects_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])",
            "CONSTRAINT [FK_UserProjects_Project] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id])"
        })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "BIGINT", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("UserId", DataType = "BIGINT", NotNull = true)]
            public static long UserId { get; set; }

            [Column("ProjectId", DataType = "BIGINT", NotNull = true)]
            public static long ProjectId { get; set; }

            [Column("Role", DataType = "NVARCHAR(100)", NotNull = true)]
            public static string Role { get; set; }

            [Column("Allocation", DataType = "FLOAT", DefaultValue = "0", NotNull = true)]
            public static double Allocation { get; set; }

            [Column("HourlyRate", DataType = "DECIMAL(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [Column("IsActive", DataType = "BIT", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("AssignedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime AssignedAt { get; set; }

            [Column("RemovedAt", DataType = "DATETIME2")]
            public static DateTime? RemovedAt { get; set; }

            [Column("CreatedAt", DataType = "DATETIME2", NotNull = true, DefaultValue = "GETDATE()")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "DATETIME2")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.MySql;
using Drizzle4Dotnet.MySql.Schema;

namespace SharedDemo.MySql
{
    [Table("Users", Dialect = typeof(MySqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT `FK_Users_Departments` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments`(`Id`)",
            "CONSTRAINT `FK_Users_Roles` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`)",
            "CONSTRAINT `FK_Users_Manager` FOREIGN KEY (`ManagerId`) REFERENCES `Users`(`Id`)"
        })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [MySqlBigInt]
            [Column("Id", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [MySqlUuid]
            [Column("Guid", NotNull = true)]
            public static Guid Guid { get; set; }

            [MySqlText]
            [Column("Name", NotNull = true)]
            public static string Name { get; set; }

            [MySqlText]
            [Column("Email", NotNull = true)]
            public static string Email { get; set; }

            [MySqlBigInt]
            [Column("Age", NotNull = true)]
            public static long Age { get; set; }

            [MySqlDecimal]
            [Column("Salary", DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [MySqlDouble]
            [Column("Rating", NotNull = true)]
            public static double Rating { get; set; }

            [MySqlBoolean]
            [Column("IsActive", NotNull = true)]
            public static bool IsActive { get; set; }

            [MySqlBigInt]
            [Column("DepartmentId", NotNull = true)]
            public static long DepartmentId { get; set; }

            [MySqlBigInt]
            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [MySqlBigInt]
            [Column("RoleId", NotNull = true)]
            public static long RoleId { get; set; }

            [MySqlDateTime]
            [Column("CreatedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime CreatedAt { get; set; }

            [MySqlDateTime]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
            
            [MySqlDateTime]
            [Column("DeletedAt")]
            public static DateTime? DeletedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", Dialect = typeof(MySqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT `FK_Departments_Parent` FOREIGN KEY (`ParentDepartmentId`) REFERENCES `Departments`(`Id`)",
            "CONSTRAINT `FK_Departments_Manager` FOREIGN KEY (`ManagerId`) REFERENCES `Users`(`Id`)"
        })]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [MySqlBigInt]
            [Column("Id", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [MySqlUuid]
            [Column("Guid", NotNull = true)]
            public static Guid Guid { get; set; }

            [MySqlText]
            [Column("Name", NotNull = true)]
            public static string Name { get; set; }

            [MySqlVarChar(100)]
            [Column("Code", NotNull = true)]
            public static string Code { get; set; }

            [MySqlText]
            [Column("Location", NotNull = true)]
            public static string Location { get; set; }

            [MySqlDecimal]
            [Column("Budget", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [MySqlBigInt]
            [Column("HeadCount", DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [MySqlBoolean]
            [Column("IsActive", NotNull = true)]
            public static bool IsActive { get; set; }

            [MySqlDateTime]
            [Column("CreatedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime CreatedAt { get; set; }

            [MySqlDateTime]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [MySqlText]
            [Column("Description", NotNull = true)]
            public static string Description { get; set; }
            
            [MySqlBigInt]
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [MySqlBigInt]
            [Column("ManagerId", NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", Dialect = typeof(MySqlSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [MySqlBigInt]
            [Column("Id", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [MySqlUuid]
            [Column("Guid", NotNull = true)]
            public static Guid Guid { get; set; }

            [MySqlText]
            [Column("Name", NotNull = true)]
            public static string Name { get; set; }

            [MySqlBigInt]
            [Column("Level", NotNull = true)]
            public static long Level { get; set; }

            [MySqlDecimal]
            [Column("BaseSalary", DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [MySqlDouble]
            [Column("BonusRate", DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [MySqlBoolean]
            [Column("IsActive", NotNull = true)]
            public static bool IsActive { get; set; }

            [MySqlBoolean]
            [Column("CanApproveBudget", NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [MySqlDateTime]
            [Column("CreatedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime CreatedAt { get; set; }

            [MySqlDateTime]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [MySqlText]
            [Column("Description", NotNull = true)]
            public static string Description { get; set; }
        }
    }

    [Table("Projects", Dialect = typeof(MySqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT `FK_Projects_Departments` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments`(`Id`)",
            "CONSTRAINT `FK_Projects_Owner` FOREIGN KEY (`OwnerId`) REFERENCES `Users`(`Id`)"
        })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [MySqlBigInt]
            [Column("Id", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [MySqlUuid]
            [Column("Guid", NotNull = true)]
            public static Guid Guid { get; set; }

            [MySqlText]
            [Column("Name", NotNull = true)]
            public static string Name { get; set; }

            [MySqlVarChar(100)]
            [Column("Code", NotNull = true)]
            public static string Code { get; set; }

            [MySqlBigInt]
            [Column("OwnerId", NotNull = true)]
            public static long OwnerId { get; set; }

            [MySqlBigInt]
            [Column("DepartmentId", NotNull = true)]
            public static long DepartmentId { get; set; }

            [MySqlDecimal]
            [Column("Budget", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [MySqlDouble]
            [Column("Progress", DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [MySqlBoolean]
            [Column("IsActive", NotNull = true)]
            public static bool IsActive { get; set; }

            [MySqlDateTime]
            [Column("StartDate", NotNull = true)]
            public static DateTime StartDate { get; set; }

            [MySqlDateTime]
            [Column("EndDate")]
            public static DateTime? EndDate { get; set; }

            [MySqlDateTime]
            [Column("CreatedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime CreatedAt { get; set; }

            [MySqlDateTime]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }

    [Table("UserProjects", Dialect = typeof(MySqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT `FK_UserProjects_User` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`)",
            "CONSTRAINT `FK_UserProjects_Project` FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`)"
        })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [MySqlBigInt]
            [Column("Id", PrimaryKey = true, AutoIncrement = true)]
            public static long Id { get; set; }

            [MySqlBigInt]
            [Column("UserId", NotNull = true)]
            public static long UserId { get; set; }

            [MySqlBigInt]
            [Column("ProjectId", NotNull = true)]
            public static long ProjectId { get; set; }

            [MySqlVarChar(100)]
            [Column("Role", NotNull = true)]
            public static string Role { get; set; }

            [MySqlDouble]
            [Column("Allocation", DefaultValue = "0", NotNull = true)]
            public static double Allocation { get; set; }

            [MySqlDecimal]
            [Column("HourlyRate", DefaultValue = "0", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [MySqlBoolean]
            [Column("IsActive", NotNull = true)]
            public static bool IsActive { get; set; }

            [MySqlDateTime]
            [Column("AssignedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime AssignedAt { get; set; }

            [MySqlDateTime]
            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [MySqlDateTime]
            [Column("CreatedAt", NotNull = true, DefaultValue = "CURRENT_TIMESTAMP(6)")]
            public static DateTime CreatedAt { get; set; }

            [MySqlDateTime]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

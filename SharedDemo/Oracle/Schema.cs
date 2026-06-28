using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace SharedDemo.Oracle
{
    [Table("Users", null, Dialect = typeof(OracleSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT \"FK_Users_Departments\" FOREIGN KEY (\"DepartmentId\") REFERENCES \"Departments\"(\"Id\")",
            "CONSTRAINT \"FK_Users_Roles\" FOREIGN KEY (\"RoleId\") REFERENCES \"Roles\"(\"Id\")",
            "CONSTRAINT \"FK_Users_Manager\" FOREIGN KEY (\"ManagerId\") REFERENCES \"Users\"(\"Id\")"
        })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "NUMBER(10)", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "RAW(16)", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "VARCHAR2(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Email", DataType = "VARCHAR2(255)", NotNull = true)]
            public static string Email { get; set; }

            [Column("Age", DataType = "NUMBER(10)", NotNull = true)]
            public static long Age { get; set; }

            [Column("Salary", DataType = "NUMBER(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Salary { get; set; }

            [Column("Rating", DataType = "BINARY_DOUBLE", NotNull = true)]
            public static double Rating { get; set; }

            [Column("IsActive", DataType = "NUMBER(1)", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("DepartmentId", DataType = "NUMBER(10)", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("ManagerId", DataType = "NUMBER(10)")]
            public static long? ManagerId { get; set; }

            [Column("RoleId", DataType = "NUMBER(10)", NotNull = true)]
            public static long RoleId { get; set; }

            [Column("CreatedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TIMESTAMP")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Alias(typeof(UsersTable), "Manager", Dialect = typeof(OracleSqlDialectImpl))]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", null, Dialect = typeof(OracleSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT \"FK_Departments_Parent\" FOREIGN KEY (\"ParentDepartmentId\") REFERENCES \"Departments\"(\"Id\")",
            "CONSTRAINT \"FK_Departments_Manager\" FOREIGN KEY (\"ManagerId\") REFERENCES \"Users\"(\"Id\")"
        })]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "NUMBER(10)", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "RAW(16)", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "VARCHAR2(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "VARCHAR2(100)", NotNull = true)]
            public static string Code { get; set; }

            [Column("Location", DataType = "VARCHAR2(255)", NotNull = true)]
            public static string Location { get; set; }

            [Column("Budget", DataType = "NUMBER(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("HeadCount", DataType = "NUMBER(10)", DefaultValue = "0", NotNull = true)]
            public static long HeadCount { get; set; }

            [Column("IsActive", DataType = "NUMBER(1)", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CreatedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TIMESTAMP")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "CLOB", NotNull = true)]
            public static string Description { get; set; }
            
            [Column("ParentDepartmentId", DataType = "NUMBER(10)")]
            public static long? ParentDepartmentId { get; set; }
            
            [Column("ManagerId", DataType = "NUMBER(10)", NotNull = true)]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", null, Dialect = typeof(OracleSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "NUMBER(10)", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "RAW(16)", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "VARCHAR2(100)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Level", DataType = "NUMBER(10)", NotNull = true)]
            public static long Level { get; set; }

            [Column("BaseSalary", DataType = "NUMBER(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal BaseSalary { get; set; }

            [Column("BonusRate", DataType = "BINARY_DOUBLE", DefaultValue = "0", NotNull = true)]
            public static double BonusRate { get; set; }

            [Column("IsActive", DataType = "NUMBER(1)", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("CanApproveBudget", DataType = "NUMBER(1)", NotNull = true)]
            public static bool CanApproveBudget { get; set; }

            [Column("CreatedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TIMESTAMP")]
            public static DateTime? UpdatedAt { get; set; }

            [Column("Description", DataType = "CLOB", NotNull = true)]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", null, Dialect = typeof(OracleSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT \"FK_Projects_Departments\" FOREIGN KEY (\"DepartmentId\") REFERENCES \"Departments\"(\"Id\")",
            "CONSTRAINT \"FK_Projects_Owner\" FOREIGN KEY (\"OwnerId\") REFERENCES \"Users\"(\"Id\")"
        })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "NUMBER(10)", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("Guid", DataType = "RAW(16)", NotNull = true)]
            public static Guid Guid { get; set; }

            [Column("Name", DataType = "VARCHAR2(255)", NotNull = true)]
            public static string Name { get; set; }

            [Column("Code", DataType = "VARCHAR2(100)", NotNull = true)]
            public static string Code { get; set; }

            [Column("OwnerId", DataType = "NUMBER(10)", NotNull = true)]
            public static long OwnerId { get; set; }

            [Column("DepartmentId", DataType = "NUMBER(10)", NotNull = true)]
            public static long DepartmentId { get; set; }

            [Column("Budget", DataType = "NUMBER(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal Budget { get; set; }

            [Column("Progress", DataType = "BINARY_DOUBLE", DefaultValue = "0", NotNull = true)]
            public static double Progress { get; set; }

            [Column("IsActive", DataType = "NUMBER(1)", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("StartDate", DataType = "DATE", NotNull = true)]
            public static DateTime StartDate { get; set; }

            [Column("EndDate", DataType = "DATE")]
            public static DateTime? EndDate { get; set; }

            [Column("CreatedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TIMESTAMP")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects", null, Dialect = typeof(OracleSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT \"FK_UserProjects_User\" FOREIGN KEY (\"UserId\") REFERENCES \"Users\"(\"Id\")",
            "CONSTRAINT \"FK_UserProjects_Project\" FOREIGN KEY (\"ProjectId\") REFERENCES \"Projects\"(\"Id\")"
        })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [Column("Id", DataType = "NUMBER(10)", PrimaryKey = true)]
            public static long Id { get; set; }

            [Column("UserId", DataType = "NUMBER(10)", NotNull = true)]
            public static long UserId { get; set; }

            [Column("ProjectId", DataType = "NUMBER(10)", NotNull = true)]
            public static long ProjectId { get; set; }

            [Column("Role", DataType = "VARCHAR2(100)", NotNull = true)]
            public static string Role { get; set; }

            [Column("Allocation", DataType = "BINARY_DOUBLE", DefaultValue = "0", NotNull = true)]
            public static double Allocation { get; set; }

            [Column("HourlyRate", DataType = "NUMBER(18,2)", DefaultValue = "0", NotNull = true)]
            public static decimal HourlyRate { get; set; }

            [Column("IsActive", DataType = "NUMBER(1)", NotNull = true)]
            public static bool IsActive { get; set; }

            [Column("AssignedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime AssignedAt { get; set; }

            [Column("RemovedAt", DataType = "TIMESTAMP")]
            public static DateTime? RemovedAt { get; set; }

            [Column("CreatedAt", DataType = "TIMESTAMP", NotNull = true, DefaultValue = "SYSTIMESTAMP")]
            public static DateTime CreatedAt { get; set; }

            [Column("UpdatedAt", DataType = "TIMESTAMP")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

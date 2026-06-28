using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.PgSql;
using Drizzle4Dotnet.PgSql.Schema;

namespace SharedDemo.PgSql
{
    [Table("Users", "public", Dialect = typeof(PgSqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Users_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Roles", new[] { "RoleId" }, typeof(RolesTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Users_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    [Index("IX_Users_Email", new[] { "Email" }, IsUnique = true)]
    [Index("IX_Users_Name_Age", new[] { "Name", "Age" })]
    public partial class UsersTable
    {
        public static class Columns
        {
            [PgSqlBigInt]
            [Column("Id")]
            [PrimaryKey]
            public static long Id { get; set; }

            [PgSqlUuid]
            [Column("Guid")]
            [NotNull]
            public static Guid Guid { get; set; }

            [PgSqlText]
            [Column("Name")]
            [NotNull]
            public static string Name { get; set; }

            [PgSqlText]
            [Column("Email")]
            [NotNull]
            public static string Email { get; set; }

            [PgSqlBigInt]
            [Column("Age")]
            [NotNull]
            public static long Age { get; set; }

            [PgSqlNumeric]
            [Column("Salary")]
            [NotNull]
            [DefaultValue("0")]
            public static decimal Salary { get; set; }

            [PgSqlDoublePrecision]
            [Column("Rating")]
            [NotNull]
            public static double Rating { get; set; }

            [PgSqlBoolean]
            [Column("IsActive")]
            [NotNull]
            public static bool IsActive { get; set; }

            [PgSqlBigInt]
            [Column("DepartmentId")]
            [NotNull]
            public static long DepartmentId { get; set; }

            [PgSqlBigInt]
            [Column("ManagerId")]
            public static long? ManagerId { get; set; }

            [PgSqlBigInt]
            [Column("RoleId")]
            [NotNull]
            public static long RoleId { get; set; }

            [PgSqlTimestamp]
            [Column("CreatedAt")]
            [NotNull]
            [DefaultValue("NOW()")]
            public static DateTime CreatedAt { get; set; }

            [PgSqlTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }

    [Alias(typeof(UsersTable), "Manager", Dialect = typeof(PgSqlSqlDialectImpl))]
    public partial class ManagersTable
    {
    }
    
    [Table("Departments", "public", Dialect = typeof(PgSqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Departments_Parent", new[] { "ParentDepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Departments_Manager", new[] { "ManagerId" }, typeof(UsersTable), new[] { "Id" })]
    [Index("IX_Departments_Name", new[] { "Name" }, IsUnique = true)]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [PgSqlBigInt]
            [Column("Id")]
            [PrimaryKey]
            public static long Id { get; set; }

            [PgSqlUuid]
            [Column("Guid")]
            [NotNull]
            public static Guid Guid { get; set; }

            [PgSqlText]
            [Column("Name")]
            [NotNull]
            public static string Name { get; set; }

            [PgSqlText]
            [Column("Code")]
            [NotNull]
            public static string Code { get; set; }

            [PgSqlText]
            [Column("Location")]
            [NotNull]
            public static string Location { get; set; }

            [PgSqlNumeric]
            [Column("Budget")]
            [NotNull]
            [DefaultValue("0")]
            public static decimal Budget { get; set; }

            [PgSqlBigInt]
            [Column("HeadCount")]
            [NotNull]
            [DefaultValue("0")]
            public static long HeadCount { get; set; }

            [PgSqlBoolean]
            [Column("IsActive")]
            [NotNull]
            public static bool IsActive { get; set; }

            [PgSqlTimestamp]
            [Column("CreatedAt")]
            [NotNull]
            [DefaultValue("NOW()")]
            public static DateTime CreatedAt { get; set; }

            [PgSqlTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [PgSqlText]
            [Column("Description")]
            [NotNull]
            public static string Description { get; set; }
            
            [PgSqlBigInt]
            [Column("ParentDepartmentId")]
            public static long? ParentDepartmentId { get; set; }
            
            [PgSqlBigInt]
            [Column("ManagerId")]
            [NotNull]
            public static long ManagerId { get; set; }
        }
    }
    
    [Table("Roles", "public", Dialect = typeof(PgSqlSqlDialectImpl))]
    public partial class RolesTable
    {
        public static class Columns
        {
            [PgSqlBigInt]
            [Column("Id")]
            public static long Id { get; set; }

            [PgSqlUuid]
            [Column("Guid")]
            public static Guid Guid { get; set; }

            [PgSqlText]
            [Column("Name")]
            public static string Name { get; set; }

            [PgSqlBigInt]
            [Column("Level")]
            public static long Level { get; set; }

            [PgSqlNumeric]
            [Column("BaseSalary")]
            public static decimal BaseSalary { get; set; }

            [PgSqlDoublePrecision]
            [Column("BonusRate")]
            public static double BonusRate { get; set; }

            [PgSqlBoolean]
            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [PgSqlBoolean]
            [Column("CanApproveBudget")]
            public static bool CanApproveBudget { get; set; }

            [PgSqlTimestamp]
            [Column("CreatedAt", NotNull = true, DefaultValue = "NOW()")]
            public static DateTime CreatedAt { get; set; }

            [PgSqlTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }

            [PgSqlText]
            [Column("Description")]
            public static string Description { get; set; }
        }
    }
    
    [Table("Projects", "public", Dialect = typeof(PgSqlSqlDialectImpl))]
    [ForeignKeyConstraint("FK_Projects_Departments", new[] { "DepartmentId" }, typeof(DepartmentsTable), new[] { "Id" })]
    [ForeignKeyConstraint("FK_Projects_Owner", new[] { "OwnerId" }, typeof(UsersTable), new[] { "Id" })]
    [CheckTableConstraint("Progress >= 0 AND Progress <= 100")]
    [Index("IX_Projects_Code", new[] { "Code" }, IsUnique = true)]
    [Index("IX_Projects_OwnerId", new[] { "OwnerId" })]
    public partial class ProjectsTable
    {
        public static class Columns
        {
            [PgSqlBigInt]
            [Column("Id")]
            [PrimaryKey]
            public static long Id { get; set; }

            [PgSqlUuid]
            [Column("Guid")]
            [NotNull]
            public static Guid Guid { get; set; }

            [PgSqlText]
            [Column("Name")]
            [NotNull]
            public static string Name { get; set; }

            [PgSqlText]
            [Column("Code")]
            [NotNull]
            public static string Code { get; set; }

            [PgSqlBigInt]
            [Column("OwnerId")]
            [NotNull]
            public static long OwnerId { get; set; }

            [PgSqlBigInt]
            [Column("DepartmentId")]
            [NotNull]
            public static long DepartmentId { get; set; }

            [PgSqlNumeric]
            [Column("Budget")]
            [NotNull]
            [DefaultValue("0")]
            public static decimal Budget { get; set; }

            [PgSqlDoublePrecision]
            [Column("Progress")]
            [NotNull]
            [DefaultValue("0")]
            public static double Progress { get; set; }

            [PgSqlBoolean]
            [Column("IsActive")]
            [NotNull]
            public static bool IsActive { get; set; }

            [PgSqlTimestamp]
            [Column("StartDate")]
            [NotNull]
            public static DateTime StartDate { get; set; }

            [PgSqlTimestamp]
            [Column("EndDate")]
            public static DateTime? EndDate { get; set; }

            [PgSqlTimestamp]
            [Column("CreatedAt")]
            [NotNull]
            [DefaultValue("NOW()")]
            public static DateTime CreatedAt { get; set; }

            [PgSqlTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
    
    [Table("UserProjects",  "public", Dialect = typeof(PgSqlSqlDialectImpl),
        Constraints = new[] {
            "CONSTRAINT \"FK_UserProjects_User\" FOREIGN KEY (\"UserId\") REFERENCES \"Users\"(\"Id\")",
            "CONSTRAINT \"FK_UserProjects_Project\" FOREIGN KEY (\"ProjectId\") REFERENCES \"Projects\"(\"Id\")"
        })]
    public partial class UserProjectsTable
    {
        public static class Columns
        {
            [PgSqlBigInt]
            [Column("Id")]
            public static long Id { get; set; }

            [PgSqlBigInt]
            [Column("UserId")]
            public static long UserId { get; set; }

            [PgSqlBigInt]
            [Column("ProjectId")]
            public static long ProjectId { get; set; }

            [PgSqlText]
            [Column("Role")]
            public static string Role { get; set; }

            [PgSqlDoublePrecision]
            [Column("Allocation")]
            public static double Allocation { get; set; }

            [PgSqlNumeric]
            [Column("HourlyRate")]
            public static decimal HourlyRate { get; set; }

            [PgSqlBoolean]
            [Column("IsActive")]
            public static bool IsActive { get; set; }

            [PgSqlTimestamp]
            [Column("AssignedAt", NotNull = true, DefaultValue = "NOW()")]
            public static DateTime AssignedAt { get; set; }

            [PgSqlTimestamp]
            [Column("RemovedAt")]
            public static DateTime? RemovedAt { get; set; }

            [PgSqlTimestamp]
            [Column("CreatedAt", NotNull = true, DefaultValue = "NOW()")]
            public static DateTime CreatedAt { get; set; }

            [PgSqlTimestamp]
            [Column("UpdatedAt")]
            public static DateTime? UpdatedAt { get; set; }
        }
    }
}

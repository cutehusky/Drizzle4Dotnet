using Drizzle_Like.Schema.Columns;
using Drizzle_Like.Schema.Tables;

namespace MyNamespace
{
    [Table("Users")]
    public partial class UsersTable: DbTable
    {
        public static class Columns
        {
            [PrimaryKey]
            [Column("Id")]
            public static DbColumn<int> Id { get; set; }

            [Column("Name")]
            public static DbColumn<string> Name { get; set; }

            [Column("Email")]
            public static DbColumn<string> Email { get; set; }
        
            [Column("DepartmentId")]
            public static DbColumn<int> DepartmentId { get; set; }
            
            [Column("ManagerId")]
            public static DbColumn<int> ManagerId { get; set; }
        }
    }

    [Alias(typeof(UsersTable), "Manager")]
    public partial class ManagersTable: DbTable
    {
    
    }

    [Table("Departments")]
    public partial class DepartmentsTable: DbTable
    {
        public static class Columns
        {
            [PrimaryKey] [Column("Id")] public static DbColumn<int> Id { get; set; }

            [Column("Name")] public static DbColumn<string> Name { get; set; }

            [Column("Email")] public static DbColumn<string> Email { get; set; }
        }
    }
}
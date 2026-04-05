using Drizzle4Dotnet.Schema.Columns;
using Drizzle4Dotnet.Schema.Tables;

namespace MyNamespace
{
    [Table("Users")]
    public partial class UsersTable
    {
        public static class Columns
        {
            [PrimaryKey]
            [Column("Id")]
            public static IColumn<int> Id { get; set; }

            [Column("Name")]
            public static IColumn<string> Name { get; set; }

            [Column("Email")]
            public static IColumn<string> Email { get; set; }
        
            [Column("DepartmentId")]
            public static IColumn<int> DepartmentId { get; set; }
            
            [Column("ManagerId")]
            public static IColumn<int> ManagerId { get; set; }
        }
    }

    [Alias(typeof(UsersTable), "Manager")]
    public partial class ManagersTable
    {
    
    }

    [Table("Departments")]
    public partial class DepartmentsTable
    {
        public static class Columns
        {
            [PrimaryKey] [Column("Id")] public static IColumn<int> Id { get; set; }

            [Column("Name")] public static IColumn<string> Name { get; set; }

            [Column("Email")] public static IColumn<string> Email { get; set; }
        }
    }
}
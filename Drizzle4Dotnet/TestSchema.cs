using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace MyNamespace
{
    [Table("Users")]
    public partial class UsersTable
    {
        public static class Columns
        {
            [Column("Id")]
            public static int Id { get; set; }

            [Column("Name")]
            public static string Name { get; set; }

            [Column("Email")]
            public static string Email { get; set; }
        
            [Column("DepartmentId")]
            public static int DepartmentId { get; set; }
            
            [Column("ManagerId")]
            public static int ManagerId { get; set; }
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
            [Column("Id")] 
            public static int Id { get; set; }

            [Column("Name")] 
            public static string Name { get; set; }

            [Column("Email")] 
            public static string Email { get; set; }
        }
    }
}
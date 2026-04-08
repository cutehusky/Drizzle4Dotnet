using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Microsoft.EntityFrameworkCore;
using MyNamespace;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}


[DbSelect]
public static partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public static int Id { get; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public static string Email { get; }
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public static string Name { get; }
}

namespace MyNamespace
{
    [Table("Users")]
    public partial class UsersTable
    {
        public static class Columns
        {
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
            [Column("Id")] public static IColumn<int> Id { get; set; }

            [Column("Name")] public static IColumn<string> Name { get; set; }

            [Column("Email")] public static IColumn<string> Email { get; set; }
        }
    }
}


public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
    }
}

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
public partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set; }
}

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

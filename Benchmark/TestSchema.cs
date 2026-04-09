using Microsoft.EntityFrameworkCore;

public class User
{
    public  int Id { get; set; }

    public  Guid Guid { get; set; }

    public  string Name { get; set; }

    public  string Email { get; set; }

    public  int Age { get; set; }

    public  decimal Salary { get; set; }

    public  double Rating { get; set; }

    public  bool IsActive { get; set; }

    public  int DepartmentId { get; set; }

    public  int? ManagerId { get; set; }

    public  int RoleId { get; set; }

    public  DateTime CreatedAt { get; set; }

    public  DateTime? UpdatedAt { get; set; }
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

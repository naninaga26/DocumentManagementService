using Microsoft.EntityFrameworkCore;
using Document.Services.AuthAPI.Models;
namespace Document.Services.AuthAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and constraints
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
        });
        // Seed Roles if using an EnumToStringConverter or if Role was an entity
        modelBuilder.Entity<User>()
            .Property(u => u.UserRole)
            .HasConversion<string>(); // Store enum as string


    }
}

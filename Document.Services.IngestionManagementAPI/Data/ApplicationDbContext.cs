using Microsoft.EntityFrameworkCore;
using Document.Services.IngestionManagementAPI.Models;
namespace Document.Services.IngestionManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<IngestionProcess> IngestionProcesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure relationships and constraints
    }
}

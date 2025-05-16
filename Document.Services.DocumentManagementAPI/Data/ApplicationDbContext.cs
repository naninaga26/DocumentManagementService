using Microsoft.EntityFrameworkCore;
using Document.Services.DocumentManagementAPI.Models;
namespace Document.Services.DocumentManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FileMetaData> FilesMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure relationships and constraints
    }
}

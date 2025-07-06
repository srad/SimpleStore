using Microsoft.EntityFrameworkCore;

namespace SimpleStore.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<BucketFile> BucketFiles { get; set; }
    public DbSet<Bucket> Buckets { get; set; }
    public DbSet<AllowedHost> AllowedHosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Bucket>()
            .HasMany(b => b.Files)
            .WithOne(f => f.Bucket)
            .HasForeignKey(f => f.BucketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
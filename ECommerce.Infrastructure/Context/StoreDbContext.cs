using ECommerce.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Context;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasMany(o => o.BasketItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.BasketItems)
                .HasForeignKey(oi => oi.OrderId)
                .IsRequired(false); 
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        var projectDir = Directory.GetCurrentDirectory();
        var rootDir = Path.Combine(projectDir, "..");
        var envPath = Path.Combine(rootDir, ".env");
        DotNetEnv.Env.Load(envPath);

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "DB_CONNECTION_STRING environment variable not found. Please ensure it is set in the .env file.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<StoreDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new StoreDbContext(optionsBuilder.Options);
    }
}

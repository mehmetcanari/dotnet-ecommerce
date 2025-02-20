using Microsoft.EntityFrameworkCore;

namespace OnlineStoreWeb.API.Model;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions options) : base(options) { }
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Account> Accounts => Set<Account>();
}
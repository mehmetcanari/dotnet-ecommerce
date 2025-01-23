using Microsoft.EntityFrameworkCore;

namespace OnlineStoreWeb.API.Model;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Account> Accounts => Set<Account>();
}
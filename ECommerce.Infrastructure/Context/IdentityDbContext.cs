using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Context;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>().ToTable("Users", "Identity");
        builder.Entity<IdentityRole>().ToTable("Roles", "Identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "Identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "Identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "Identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "Identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "Identity");
    }
}

public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        EnvConfig.LoadEnv();
        
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "DB_CONNECTION_STRING environment variable not found. Please ensure it is set in the .env file.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new IdentityDbContext(optionsBuilder.Options);
    }
}

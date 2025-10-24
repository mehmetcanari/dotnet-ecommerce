using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Context;

public class ApplicationIdentityDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options){}

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

public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
    {
        EnvConfig.LoadEnv();
        
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "DB_CONNECTION_STRING environment variable not found. Please ensure it is set in the .env file.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}

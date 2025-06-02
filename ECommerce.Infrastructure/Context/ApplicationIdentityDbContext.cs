using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Context;

public class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) 
    : IdentityDbContext<IdentityUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<IdentityUser>().ToTable("Users", "Identity");
        builder.Entity<IdentityRole>().ToTable("Roles", "Identity");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "Identity");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "Identity");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "Identity");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "Identity");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "Identity");
    }
}

public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
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

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}

using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.API.Extensions;

public static class DatabaseSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedDatabaseAsync(IHost app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogService>();
            var unitOfWork = services.GetRequiredService<ICrossContextUnitOfWork>();
            try
            {
                DotNetEnv.Env.Load();

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                await SeedRolesAsync(roleManager);

                var userManager = services.GetRequiredService<UserManager<User>>();
                var accountRepository = services.GetRequiredService<IAccountRepository>();

                await SeedAdminUserAsync(userManager, accountRepository, logger, unitOfWork);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ErrorMessages.ErrorSeedingAdminUser);
                throw;
            }
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roleNames = { AdminRole };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager, IAccountRepository accountRepository, ILogService logger, ICrossContextUnitOfWork unitOfWork)
    {
        string adminEmail = DotNetEnv.Env.GetString("ADMIN_EMAIL");
        string adminPassword = DotNetEnv.Env.GetString("ADMIN_PASSWORD");

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            return;

        await unitOfWork.BeginTransactionAsync();

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Name = "Admin",
                Surname = "Account",
                IdentityNumber = "00000000000",
                PhoneNumber = "0000000000",
                City = "Default",
                Country = "Default",
                ZipCode = "00000",
                Address = "Default",
                DateOfBirth = DateTime.UtcNow.AddYears(-30)
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                await accountRepository.CreateAsync(adminUser, CancellationToken.None);
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
            else
            {
                await unitOfWork.RollbackTransaction();
            }

            await unitOfWork.CommitTransactionAsync();
        }
        else
        {
            logger.LogInformation(ErrorMessages.AdminAccountAlreadyExists);
            await unitOfWork.RollbackTransaction();
        }
    }
}
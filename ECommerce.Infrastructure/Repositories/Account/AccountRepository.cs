using ECommerce.Application.Interfaces.Repository;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.Account;

public class AccountRepository(StoreDbContext context) : IAccountRepository
{
    public async Task<List<Domain.Model.Account>> Read()
    {
        try
        {
            return await context.Accounts.AsNoTracking().ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch accounts", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Create(Domain.Model.Account userAccount)
    {
        try
        {
            await context.Accounts.AddAsync(userAccount);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save account", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Update(Domain.Model.Account account)
    {
        try
        {
            context.Accounts.Update(account);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update account", ex);
        }
    }

    public async Task Delete(Domain.Model.Account account)
    {
        try
        {
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete account", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
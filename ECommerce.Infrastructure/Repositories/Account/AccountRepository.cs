using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.Account;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public AccountRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Domain.Model.Account>> Read()
    {
        try
        {
            return await _context.Accounts.AsNoTracking().ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to fetch accounts");
            throw new DbUpdateException("Failed to fetch accounts", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Create(Domain.Model.Account userAccount)
    {
        try
        {
            await _context.Accounts.AddAsync(userAccount);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save account");
            throw new DbUpdateException("Failed to save account", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Update(Domain.Model.Account account)
    {
        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update account");
            throw new DbUpdateException("Failed to update account", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Delete(Domain.Model.Account account)
    {
        try
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete account");
            throw new DbUpdateException("Failed to delete account", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
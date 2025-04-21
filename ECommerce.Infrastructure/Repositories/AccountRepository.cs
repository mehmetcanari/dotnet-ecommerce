using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public AccountRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Account>> Read()
    {
        try
        {
            return await _context.Accounts
            .AsNoTracking()
            .ToListAsync();
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

    public async Task Create(Account userAccount)
    {
        try
        {
            await _context.Accounts.AddAsync(userAccount);
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

    public void Update(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
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

    public void Delete(Account account)
    {
        try
        {
            _context.Accounts.Remove(account);
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
using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

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
            IQueryable<Account> query = _context.Accounts;

            var accounts = await query
            .AsNoTracking()
            .ToListAsync();

            return accounts;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
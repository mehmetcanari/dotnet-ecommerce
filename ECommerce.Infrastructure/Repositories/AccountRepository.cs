using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;

    public AccountRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> Read(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            IQueryable<Account> query = _context.Accounts;

            var accounts = await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return accounts;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while reading accounts", exception);
        }
    }
    
    public async Task<Account?> GetAccountByEmail(string email)
    {
        try
        {
            
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Email == email)
                .FirstOrDefaultAsync();

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting account by email", exception);
        }
    }

    public async Task<Account?> GetAccountById(int id)
    {
        try
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting account by id", exception);
        }
    }

    public async Task Create(Account userAccount)
    {
        try
        {
            await _context.Accounts.AddAsync(userAccount);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating account", exception);
        }
    }

    public void Update(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating account", exception);
        }
    }

    public void Delete(Account account)
    {
        try
        {
            _context.Accounts.Remove(account);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting account", exception);
        }
    }
}
using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;

namespace ECommerce.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;

    public AccountRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<User> query = _context.Accounts;

            var accounts = await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return accounts;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<User> GetAccountByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Email == email)
                .FirstOrDefaultAsync(cancellationToken);

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<User> GetAccountById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<User> GetAccountByIdentityNumber(string identityNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.IdentityNumber == identityNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Create(User userAccount, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Accounts.AddAsync(userAccount, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public void Update(User account)
    {
        try
        {
            _context.Accounts.Update(account);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public void Delete(User account)
    {
        try
        {
            _context.Accounts.Remove(account);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
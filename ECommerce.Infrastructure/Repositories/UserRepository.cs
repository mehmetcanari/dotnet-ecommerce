using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;

namespace ECommerce.Infrastructure.Repositories;

public class UserRepository(StoreDbContext context) : IUserRepository
{
    public async Task<List<User>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<User> query = context.Accounts;

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

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            
            var account = await context.Accounts
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

    public async Task<User?> GetById(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await context.Accounts
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            return account;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task CreateAsync(User userAccount, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Accounts.AddAsync(userAccount, cancellationToken);
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
            context.Accounts.Update(account);
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
            context.Accounts.Remove(account);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
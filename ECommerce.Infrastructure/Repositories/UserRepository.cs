using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using DbContext = ECommerce.Infrastructure.Context.DbContext;

namespace ECommerce.Infrastructure.Repositories;

public class UserRepository(DbContext context) : IUserRepository
{
    public async Task<List<User>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<User> query = context.Users;

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
            
            var account = await context.Users
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
            var account = await context.Users
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
            await context.Users.AddAsync(userAccount, cancellationToken);
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
            context.Users.Update(account);
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
            context.Users.Remove(account);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
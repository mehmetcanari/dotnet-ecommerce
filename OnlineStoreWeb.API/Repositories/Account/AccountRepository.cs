using Microsoft.EntityFrameworkCore;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;

    public AccountRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> Get()
    {
        try
        {
            return await _context.Accounts.AsNoTracking().ToListAsync();
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

    public async Task Add(Account userAccount)
    {
        try
        {
            await _context.Accounts.AddAsync(userAccount);
            await _context.SaveChangesAsync();
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

    public async Task Update(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update account", ex);
        }
    }

    public async Task Delete(Account account)
    {
        try
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
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
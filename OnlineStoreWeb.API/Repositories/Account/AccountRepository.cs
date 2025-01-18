using Microsoft.EntityFrameworkCore;

public class AccountRepository : IAccountRepository
{
    private readonly StoreDbContext _context;

    public AccountRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        try
        {
            return await _context.Accounts.ToListAsync();
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

    public async Task<Account?> GetAccountWithIdAsync(int id)
    {
        try
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch account", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddAccountAsync(AccountRegisterDto createUserRequest)
    {
        try
        {
            Account account = new Account
            {
                FullName = createUserRequest.FullName,
                Email = createUserRequest.Email,
                Password = createUserRequest.Password,
                Address = createUserRequest.Address,
                PhoneNumber = createUserRequest.PhoneNumber,
                DateOfBirth = createUserRequest.DateOfBirth,
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow
            };
            await _context.Accounts.AddAsync(account);
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

    public async Task UpdateAccountAsync(int id, AccountUpdateDto updateUserRequest)
    {
        try
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("User not found");

            account.Email = updateUserRequest.Email;
            account.Password = updateUserRequest.Password;
            account.Address = updateUserRequest.Address;
            account.PhoneNumber = updateUserRequest.PhoneNumber;
            account.UserUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update account", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        try
        {
            Account account = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("Account not found");

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
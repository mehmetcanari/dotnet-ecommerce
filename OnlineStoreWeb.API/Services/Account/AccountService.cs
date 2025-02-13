using OnlineStoreWeb.API.DTO.User;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Services.Cryptography;

namespace OnlineStoreWeb.API.Services.Account;

public class AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger) : IAccountService
{
    private readonly PasswordEncryptionProvider _passwordEncryptionProvider = new();
    
    public async Task AddAccountAsync(AccountRegisterDto createUserDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            if(accounts.Any(a => a.Email == createUserDto.Email)) //Duplicate email check
            {
                logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }

            Model.Account account = new Model.Account
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                PasswordHash = _passwordEncryptionProvider.HashPassword(createUserDto.Password),
                Address = createUserDto.Address,
                PhoneNumber = createUserDto.PhoneNumber,
                DateOfBirth = createUserDto.DateOfBirth,
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow
            };

            await accountRepository.Add(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task PartialUpdateAccountAsync(int id, AccountPatchDto accountPatchDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            
            if(accounts.Any(a => a.Email == accountPatchDto.Email)) //Duplicate email check
            {
                logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }
            
            if(_passwordEncryptionProvider.VerifyPassword(accountPatchDto.CurrentPassword, account.PasswordHash) == false)
            {
                logger.LogError("Current password is incorrect");
                throw new Exception("Current password is incorrect");
            }
            
            account.Email = accountPatchDto.Email;
            account.PasswordHash = _passwordEncryptionProvider.HashPassword(accountPatchDto.NewPassword);
            account.UserUpdated = DateTime.UtcNow;

            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating account: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            
            if(accounts.Any(a => a.Email == updateUserDto.Email)) //Duplicate email check
            {
                logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }
            
            account.FullName = updateUserDto.FullName;
            account.Email = updateUserDto.Email;
            account.PasswordHash = _passwordEncryptionProvider.HashPassword(updateUserDto.Password);
            account.Address = updateUserDto.Address;
            account.PhoneNumber = updateUserDto.PhoneNumber;
            account.UserUpdated = DateTime.UtcNow;

            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating account: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<Model.Account>> GetAllAccountsAsync()
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            int accountCount = accounts.Count;
            if(accountCount < 1)
            {
                logger.LogError("No accounts found");
                throw new Exception("No accounts found");
            }

            return accounts;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Model.Account> GetAccountWithIdAsync(int id)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            return account;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Get();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            await accountRepository.Delete(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
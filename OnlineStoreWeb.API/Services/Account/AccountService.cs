using OnlineStoreWeb.API.DTO.User;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Services.Cryptography;

namespace OnlineStoreWeb.API.Services.Account;

public class AccountService : IAccountService
{
    private readonly IAccountRepository accountRepository;
    private ILogger<AccountService> logger;
    
    public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
    {
        this.accountRepository = accountRepository;
        this.logger = logger;
    }
    
    public async Task AddAccountAsync(AccountRegisterDto createUserDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Read();
            if(accounts.Any(a => a.Email == createUserDto.Email)) //Duplicate email check
            {
                logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }

            Model.Account account = new Model.Account
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                PasswordHash = CryptographyService.HashPassword(createUserDto.Password),
                Address = createUserDto.Address,
                PhoneNumber = createUserDto.PhoneNumber,
                DateOfBirth = createUserDto.DateOfBirth,
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow
            };

            await accountRepository.Create(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task LoginAccountAsync(AccountLoginDto accountLoginDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.Email == accountLoginDto.Email) ?? throw new Exception("User not found");
            if(CryptographyService.TryVerifyPassword(accountLoginDto.Password, account.PasswordHash))
            {
                logger.LogInformation("User logged in successfully");
            }
            else
            {
                logger.LogError("Invalid password");
                throw new Exception("Invalid password");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while logging in account: {Message}", exception.Message);
            throw new Exception("An unexpected error occurred", exception);
        }
    }

    public async Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto)
    {
        try
        {
            List<Model.Account> accounts = await accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            
            if(accounts.Any(a => a.Email == updateUserDto.Email)) //Duplicate email check
            {
                logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }
            
            account.FullName = updateUserDto.FullName;
            account.Email = updateUserDto.Email;
            account.PasswordHash = CryptographyService.HashPassword(updateUserDto.Password);
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
            List<Model.Account> accounts = await accountRepository.Read();
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
            List<Model.Account> accounts = await accountRepository.Read();
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
            List<Model.Account> accounts = await accountRepository.Read();
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
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Account;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Services.Cryptography;

namespace OnlineStoreWeb.API.Services.Account;
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }
    
    public async Task AddAccountAsync(AccountRegisterDto createUserDto)
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            if(accounts.Any(a => a.Email == createUserDto.Email)) //Duplicate email check
            {
                _logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }

            Model.Account account = new Model.Account
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                PasswordHash = CryptographyService.HashPassword(createUserDto.Password),
                Address = createUserDto.Address,
                PhoneNumber = createUserDto.PhoneNumber,
                DateOfBirth = createUserDto.DateOfBirth.ToUniversalTime(),
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow
            };

            await _accountRepository.Create(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
    
    public async Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto)
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.AccountId == id) ?? throw new Exception("User not found");
            
            if(accounts.Any(a => a.Email == updateUserDto.Email)) //Duplicate email check
            {
                _logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }
            
            account.FullName = updateUserDto.FullName;
            account.Email = updateUserDto.Email;
            account.PasswordHash = CryptographyService.HashPassword(updateUserDto.Password);
            account.Address = updateUserDto.Address;
            account.PhoneNumber = updateUserDto.PhoneNumber;
            account.UserUpdated = DateTime.UtcNow;

            await _accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating account: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task LoginAccountAsync(AccountLoginDto accountLoginDto)
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.Email == accountLoginDto.Email) ?? throw new Exception("User not found");
            if(CryptographyService.TryVerifyPassword(accountLoginDto.Password, account.PasswordHash))
            {
                _logger.LogInformation("User logged in successfully");
            }
            else
            {
                _logger.LogError("Invalid password");
                throw new Exception("Invalid password");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while logging in account: {Message}", exception.Message);
            throw new Exception("An unexpected error occurred", exception);
        }
    }

    public async Task<List<AccountResponseDto>> GetAllAccountsAsync()
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            int accountCount = accounts.Count;
            if(accountCount < 1)
            {
                _logger.LogError("No accounts found");
                throw new Exception("No accounts found");
            }

            return accounts.Select(account => new AccountResponseDto
                {
                    FullName = account.FullName,
                    Email = account.Email,
                    Address = account.Address,
                    PhoneNumber = account.PhoneNumber,
                    DateOfBirth = account.DateOfBirth
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<AccountResponseDto> GetAccountWithIdAsync(int id)
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.AccountId == id) ?? throw new Exception("User not found");
            
            AccountResponseDto responseAccount = new AccountResponseDto
            {
                FullName = account.FullName,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth
            };

            return responseAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        try
        {
            List<Model.Account> accounts = await _accountRepository.Read();
            Model.Account account = accounts.FirstOrDefault(a => a.AccountId == id) ?? throw new Exception("User not found");
            await _accountRepository.Delete(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting account: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
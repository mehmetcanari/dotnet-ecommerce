using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services.Account;
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(IAccountRepository accountRepository, IRefreshTokenService refreshTokenService, UserManager<IdentityUser> userManager, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenService = refreshTokenService;
        _userManager = userManager;
        _logger = logger;
    }
    
    public async Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            if(accounts.Any(a => a.Email == createUserRequestDto.Email)) //Duplicate email check
            {
                _logger.LogError("Email already exists in the system, try another email");
                throw new Exception("Email already exists in the system, try another email");
            }

            var account = new Domain.Model.Account
            {
                FullName = createUserRequestDto.FullName,
                Email = createUserRequestDto.Email,
                Address = createUserRequestDto.Address,
                PhoneNumber = createUserRequestDto.PhoneNumber,
                DateOfBirth = createUserRequestDto.DateOfBirth.ToUniversalTime(),
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow,
                Role = role
            };

            await _accountRepository.Create(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<AccountResponseDto>> GetAllAccountsAsync()
    {
        try
        {
            var accounts = await _accountRepository.Read();
            int accountCount = accounts.Count;
            if(accountCount < 1)
            {
                _logger.LogError("No accounts found");
                throw new Exception("No accounts found");
            }

            return accounts.Select(account => new AccountResponseDto
                {
                    AccountId = account.AccountId,
                    FullName = account.FullName,
                    Email = account.Email,
                    Address = account.Address,
                    PhoneNumber = account.PhoneNumber,
                    DateOfBirth = account.DateOfBirth,
                    Role = account.Role
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Domain.Model.Account> GetAccountByEmailAsModel(string email)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("User not found");
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            throw new Exception($"User with email {email} not found", ex);
        }
    }

    public async Task<AccountResponseDto> GetAccountWithIdAsync(int id)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.AccountId == id) ?? throw new Exception("User not found");
            
            var responseAccount = new AccountResponseDto
            {
                AccountId = account.AccountId,
                FullName = account.FullName,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            };

            return responseAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.AccountId == id) ?? throw new Exception("User not found");
            var user = await _userManager.FindByEmailAsync(account.Email);
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            
            await _accountRepository.Delete(account);
            await _userManager.DeleteAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting account: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AccountResponseDto> GetAccountByEmailAsync(string email)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.Email == email) ?? 
                          throw new Exception($"User with email {email} not found");
            
            var responseAccount = new AccountResponseDto
            {
                AccountId = account.AccountId,
                FullName = account.FullName,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            };

            return responseAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            throw;
        }
    }

    public async Task BanAccountAsync(string email, DateTime until, string reason)
    {
        try
        {
            var account = await GetAccountByEmailAsModel(email);
            account.BanAccount(until, reason);
            await _accountRepository.Update(account);
            await _refreshTokenService.RevokeUserTokensAsync(email, "Account banned");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while banning account: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UnbanAccountAsync(string email)
    {
        try
        {
            var account = await GetAccountByEmailAsModel(email);
            account.UnbanAccount();
            await _accountRepository.Update(account);
            await _refreshTokenService.RevokeUserTokensAsync(email, "Account unbanned");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while unbanning account: {Message}", ex.Message);
            throw;
        }
    }
    
}
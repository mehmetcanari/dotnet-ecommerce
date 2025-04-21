using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services.Account;
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(
        IAccountRepository accountRepository, 
        IUnitOfWork unitOfWork,
        IRefreshTokenService refreshTokenService, 
        UserManager<IdentityUser> userManager, 
        ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenService = refreshTokenService;
        _userManager = userManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            if (accounts.Any(a => a.Email == createUserRequestDto.Email))
            {
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

            _logger.LogInformation("Account created successfully: {Account}", account);
            await _accountRepository.Create(account);
            await _unitOfWork.Commit();
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
            if (accountCount < 1)
            {
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
            }).ToList();
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
            throw;
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
            var user = await _userManager.FindByEmailAsync(account.Email) ?? throw new Exception("User not found");
            
            _logger.LogInformation("Account deleted successfully: {Account}", account);
            await _userManager.DeleteAsync(user);
            _accountRepository.Delete(account);
            await _unitOfWork.Commit();
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
            _accountRepository.Update(account);
            await _refreshTokenService.RevokeUserTokens(email, "Account banned");
            await _unitOfWork.Commit();
            _logger.LogInformation("Account banned successfully: {Account}", account);
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
            _accountRepository.Update(account);
            await _refreshTokenService.RevokeUserTokens(email, "Account unbanned");
            await _unitOfWork.Commit();
            _logger.LogInformation("Account unbanned successfully: {Account}", account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while unbanning account: {Message}", ex.Message);
            throw;
        }
    }
}
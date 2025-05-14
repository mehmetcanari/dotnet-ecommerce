using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
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

    public async Task<Result> RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            if (accounts.Any(a => a.Email == createUserRequestDto.Email))
            {
                return Result.Failure("Email already exists in the system, try another email");
            }

            var account = new Domain.Model.Account
            {
                Name = createUserRequestDto.Name,
                Surname = createUserRequestDto.Surname,
                Email = createUserRequestDto.Email,
                IdentityNumber = createUserRequestDto.IdentityNumber,
                City = createUserRequestDto.City,
                Country = createUserRequestDto.Country,
                ZipCode = createUserRequestDto.ZipCode,
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
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<List<AccountResponseDto>>> GetAllAccountsAsync()
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var accountCount = accounts.Count;
            if (accountCount == 0)
            {
                return Result<List<AccountResponseDto>>.Failure("No accounts found");
            }
            
            var accountList = accounts.Select(account => new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            }).ToList();
            
            return Result<List<AccountResponseDto>>.Success(accountList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            return Result<List<AccountResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result<Domain.Model.Account>> GetAccountByEmailAsEntityAsync(string email)
    {
        try
        {
            var account = await _accountRepository.GetAccountByEmail(email);
            if (account == null)
            {
                return Result<Domain.Model.Account>.Failure($"User with email {email} not found");
            }

            return Result<Domain.Model.Account>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            return Result<Domain.Model.Account>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result<AccountResponseDto>> GetAccountByEmailAsResponseAsync(string email)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.Email == email) ??
                          throw new Exception($"User with email {email} not found");

            var responseAccount = new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            };

            return Result<AccountResponseDto>.Success(responseAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result<AccountResponseDto>> GetAccountWithIdAsync(int id)
    {
        try
        {
            var account = await _accountRepository.GetAccountById(id);
            if (account == null)
            {
                return Result<AccountResponseDto>.Failure("Account not found");
            }

            var responseAccount = new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            };

            return Result<AccountResponseDto>.Success(responseAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by id: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result> DeleteAccountAsync(int id)
    {
        try
        {
            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            var user = await _userManager.FindByEmailAsync(account.Email) ?? throw new Exception("User not found");
            
            _logger.LogInformation("Account deleted successfully: {Account}", account);
            await _userManager.DeleteAsync(user);
            _accountRepository.Delete(account);
            await _unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> BanAccountAsync(string email, DateTime until, string reason)
    {
        try
        {
            var account = await GetAccountByEmailAsEntityAsync(email);
            account.Data.BanAccount(until, reason);
            _accountRepository.Update(account.Data);
            await _refreshTokenService.RevokeUserTokens(email, "Account banned");
            await _unitOfWork.Commit();
            _logger.LogInformation("Account banned successfully: {Account}", account);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while banning account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UnbanAccountAsync(string email)
    {
        try
        {
            var account = await GetAccountByEmailAsEntityAsync(email);
            account.Data.UnbanAccount();
            _accountRepository.Update(account.Data);
            await _refreshTokenService.RevokeUserTokens(email, "Account unbanned");
            await _unitOfWork.Commit();
            _logger.LogInformation("Account unbanned successfully: {Account}", account);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while unbanning account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
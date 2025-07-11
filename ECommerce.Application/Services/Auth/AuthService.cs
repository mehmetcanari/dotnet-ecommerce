using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Identity;
using ECommerce.Application.DTO.Request.Token;
using System.Security.Claims;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Application.Services.Auth;

public class AuthService : BaseValidator, IAuthService
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenUserClaimsService _tokenUserClaimsService;
    private readonly ILoggingService _logger;
    private readonly ICrossContextUnitOfWork _crossContextUnitOfWork;
    
    public AuthService(
        IAccountService accountService,
        UserManager<IdentityUser> userManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        ITokenUserClaimsService tokenUserClaimsService,
        ILoggingService logger,
        ICrossContextUnitOfWork unitOfWork,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _accountService = accountService;
        _userManager = userManager;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _tokenUserClaimsService = tokenUserClaimsService;
        _logger = logger;
        _crossContextUnitOfWork = unitOfWork;
    }

        public async Task<Result> RegisterAsync(AccountRegisterRequestDto registerRequestDto, string role)
    {
        try
        {
            var validationResult = await ValidateRegistrationRequestAsync(registerRequestDto);
            if (validationResult.IsFailure)
                return validationResult;

            await _crossContextUnitOfWork.BeginTransactionAsync();

            var accountResult = await _accountService.RegisterAccountAsync(registerRequestDto, role);
            if (accountResult.IsFailure)
            {
                _logger.LogError(new Exception("Transaction failed to commit"),"Transaction failed to commit: {Error}", accountResult.Error);
                await _crossContextUnitOfWork.RollbackTransaction();
                return accountResult.IsFailure ? Result.Failure(accountResult.Error) : Result.Success();
            }

            await _crossContextUnitOfWork.CommitTransactionAsync();

            _logger.LogInformation("User {Email} registered successfully with role {Role}", registerRequestDto.Email, role);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
            
            try
            {
                await _crossContextUnitOfWork.RollbackTransaction();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Error during rollback: {Message}", rollbackEx.Message);
            }
            
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ValidateRegistrationRequestAsync(AccountRegisterRequestDto registerRequestDto)
    {
        var validationResult = await ValidateAsync(registerRequestDto);
        if (validationResult is { IsSuccess: false, Error: not null })
            return Result.Failure(validationResult.Error);

        var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed - Email already exists: {Email}", registerRequestDto.Email);
            return Result.Failure("Email is already in use.");
        }

        return Result.Success();
    }


    public async Task<Result<AuthResponseDto>> LoginAsync(AccountLoginRequestDto loginRequestDto)
    {
        try
        {
            var validationResult = await ValidateAndReturnAsync(loginRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result<AuthResponseDto>.Failure(validationResult.Error);

            var (isValid, user) = await VerifyCredentialsAsync(loginRequestDto.Email, loginRequestDto.Password);
            if (!isValid || user == null)
            {
                return Result<AuthResponseDto>.Failure("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var authResponseDto = await RequestGenerateTokensAsync(user.Id, user.Email ?? throw new InvalidOperationException("User email cannot be null"), roles);
            _logger.LogInformation("Login successful for user: {Email}", loginRequestDto.Email);
            return Result<AuthResponseDto>.Success(authResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in: {Message}", ex.Message);
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }
    
    public async Task<Result> LogoutAsync(string reason)
    {
        try
        {
            var cookieResult = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieResult is { IsFailure: true, Error: not null }) 
                return Result.Failure(cookieResult.Error);

            var refreshToken = cookieResult.Data;
            if (refreshToken != null) 
            {
                var request = new TokenRevokeRequestDto { Email = refreshToken.Email, Reason = reason };
                await _refreshTokenService.RevokeUserTokens(request);
            }
            
            _logger.LogInformation("User logged out successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke user tokens");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResponseDto>> GenerateAuthTokenAsync()
    {
        try
        {
            var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieRefreshToken.Data is null)
            {
                _logger.LogWarning("No refresh token found in cookie");
                return Result<AuthResponseDto>.Failure("User is not logged in");
            }
            
            var identifier = _tokenUserClaimsService.GetClaimsPrincipalFromToken(cookieRefreshToken.Data);
            var (email, roles) = await _refreshTokenService.ValidateRefreshToken(identifier, _userManager);

            var userId = identifier.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Result<AuthResponseDto>.Failure("User not found");
                }
                userId = user.Id;
            }

            var authResponseDto = await RequestGenerateTokensAsync(userId, email, roles);
            return Result<AuthResponseDto>.Success(authResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user");
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    private async Task<AuthResponseDto> RequestGenerateTokensAsync(string userId, string email, IList<string> roles)
    {
        try
        {
            var accessToken = _accessTokenService.GenerateAccessTokenAsync(userId, email, roles);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, email, roles);

            if (refreshToken.Data is null)
            {
                _logger.LogWarning("Failed to generate refresh token for user: {Email}", email);
                throw new Exception("Failed to generate refresh token");
            }
            
            if (accessToken.Data is null)
            {
                _logger.LogWarning("Failed to generate access token for user: {Email}", email);
                throw new Exception("Failed to generate access token");
            }

            _refreshTokenService.SetRefreshTokenCookie(refreshToken.Data);
            Console.WriteLine( $"Refresh token: {refreshToken.Data.Token}");

            return new AuthResponseDto
            {
                AccessToken = accessToken.Data.Token,
                AccessTokenExpiration = accessToken.Data.Expires,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auth tokens");
            throw;
        }
    }

    private async Task<(bool, IdentityUser?)> VerifyCredentialsAsync(string email, string password)
    {
        try
        {
            var account = await _accountService.GetAccountByEmailAsEntityAsync(email);
            if (account.IsFailure || account.Data == null)
            {
                _logger.LogWarning("Login failed - Account not found: {Email}", email);
                return (false, null);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - User not found: {Email}", email);
                return (false, user);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed - Invalid password for user: {Email}", email);
                return (false, user);
            }

            if (account.Data is { IsBanned: true })
            {
                _logger.LogWarning("Login failed - User is banned: {Email}", email);
                throw new Exception("User is banned");
            }

            return (true, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating login process");
            throw;
        }
    }
}
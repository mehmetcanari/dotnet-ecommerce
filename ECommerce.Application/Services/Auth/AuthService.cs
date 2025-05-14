using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenUserClaimsService _tokenUserClaimsService;
    private readonly ILoggingService _logger;
    
    public AuthService(
        IAccountService accountService,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        ITokenUserClaimsService tokenUserClaimsService,
        ILoggingService logger)
    {
        _accountService = accountService;
        _userManager = userManager;
        _roleManager = roleManager;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _tokenUserClaimsService = tokenUserClaimsService;
        _logger = logger;
    }

    public async Task<Result> RegisterUserWithRoleAsync(AccountRegisterRequestDto registerRequestDto, string role)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - Email already exists: {Email}", registerRequestDto.Email);
                return Result.Failure("Email is already in use.");
            }

            var user = new IdentityUser
            {
                UserName = registerRequestDto.Email,
                Email = registerRequestDto.Email,
                PhoneNumber = registerRequestDto.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, registerRequestDto.Password);
            if (!result.Succeeded)
            {
                return Result.Failure("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    return Result.Failure($"Error creating {role} role.");
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                return Result.Failure("Error assigning role to user.");
            }

            _logger.LogInformation("User {Email} registered successfully with role {Role}", registerRequestDto.Email, role);

            await _accountService.RegisterAccountAsync(registerRequestDto, role);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(AccountLoginRequestDto loginRequestDto)
    {
        try
        {
            var (isValid, user) = await ValidateLoginProcess(loginRequestDto.Email, loginRequestDto.Password);
            if (!isValid || user == null)
            {
                return Result<AuthResponseDto>.Failure("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var authResponseDto = await RequestGenerateTokensAsync(user.Email ?? throw new InvalidOperationException("User email cannot be null"), roles);
            _logger.LogInformation("Login successful for user: {Email}", loginRequestDto.Email);
            return Result<AuthResponseDto>.Success(authResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in: {Message}", ex.Message);
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResponseDto>> GenerateAuthTokenAsync(RefreshToken cookieRefreshToken)
    {
        try
        {
            var identifier = _tokenUserClaimsService.GetClaimsPrincipalFromToken(cookieRefreshToken);

            var (email, roles) = await _refreshTokenService.ValidateRefreshToken(identifier, _userManager);

            var authResponseDto = await RequestGenerateTokensAsync(email, roles);
            return Result<AuthResponseDto>.Success(authResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user");
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    private async Task<AuthResponseDto> RequestGenerateTokensAsync(string email, IList<string> roles)
    {
        try
        {
            var accessToken = _accessTokenService.GenerateAccessTokenAsync(email, roles);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(email, roles);

            _refreshTokenService.SetRefreshTokenCookie(refreshToken);

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

    private async Task<(bool, IdentityUser?)> ValidateLoginProcess(string email, string password)
    {
        try
        {
            var account = await _accountService.GetAccountByEmailAsEntityAsync(email);

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

            if (account.Data.IsBanned)
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
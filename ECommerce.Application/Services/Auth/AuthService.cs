using System.Security.Claims;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

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

    private async Task RegisterUserWithRoleAsync(AccountRegisterRequestDto registerRequestDto, string role)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - Email already exists: {Email}", registerRequestDto.Email);
                throw new Exception("Email is already in use.");
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
                throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Error creating {role} role.");
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                throw new Exception("Error assigning role to user.");
            }

            _logger.LogInformation("User {Email} registered successfully with role {Role}", registerRequestDto.Email, role);

            await _accountService.RegisterAccountAsync(registerRequestDto, role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(AccountLoginRequestDto loginRequestDto)
    {
        try
        {
            var (isValid, user) = await ValidateLoginProcess(loginRequestDto.Email, loginRequestDto.Password);
            if (!isValid || user == null)
            {
                throw new Exception("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            AuthResponseDto authResponseDto = await RequestGenerateTokensAsync(user.Email ?? throw new InvalidOperationException("User email cannot be null"), roles);
            _logger.LogInformation("Login successful for user: {Email}", loginRequestDto.Email);
            return authResponseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AuthResponseDto> GenerateAuthTokenAsync(RefreshToken cookieRefreshToken)
    {
        try
        {
            ClaimsPrincipal identifier = await _tokenUserClaimsService.GetClaimsPrincipalFromToken(cookieRefreshToken);

            var (email, roles) = await ValidateRefreshToken(identifier);

            return await RequestGenerateTokensAsync(email, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user");
            throw;
        }
    }

    public async Task<AuthResponseDto> RequestGenerateTokensAsync(string email, IList<string> roles)
    {
        try
        {
            AccessToken accessToken = await _accessTokenService.GenerateAccessTokenAsync(email, roles);
            RefreshToken refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(email, roles);

            _refreshTokenService.SetRefreshTokenCookie(refreshToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiration = accessToken.Expires,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auth tokens");
            throw;
        }
    }

    private async Task<(string, IList<string>)> ValidateRefreshToken(ClaimsPrincipal principal)
    {
        try
        {
            var email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Email claim not found in token");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return (email, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            throw;
        }
    }

    private async Task<(bool, IdentityUser?)> ValidateLoginProcess(string email, string password)
    {
        try
        {
            var account = await _accountService.GetAccountByEmailAsModel(email);
            if (account == null)
            {
                _logger.LogWarning("Login failed - User not found: {Email}", email);
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

            if (account.IsBanned)
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

    public async Task RegisterUserAsync(AccountRegisterRequestDto registerRequestDto)
    {
        await RegisterUserWithRoleAsync(registerRequestDto, "User");
    }

    public async Task RegisterAdminAsync(AccountRegisterRequestDto registerRequestDto)
    {
        await RegisterUserWithRoleAsync(registerRequestDto, "Admin");
    }
}
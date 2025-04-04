using Microsoft.AspNetCore.Identity;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Auth;
using OnlineStoreWeb.API.Services.Account;
using OnlineStoreWeb.API.Services.Token;
using System.Security.Claims;

namespace OnlineStoreWeb.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAccountService accountService,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _accountService = accountService;
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    private async Task RegisterUserWithRoleAsync(AccountRegisterDto registerDto, string role)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed - Email already exists: {Email}", registerDto.Email);
            throw new Exception("Email is already in use.");
        }

        var user = new IdentityUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to create role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                throw new Exception($"Error creating {role} role.");
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            _logger.LogError("Failed to add role to user: {Errors}", string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            throw new Exception("Error assigning role to user.");
        }

        _logger.LogInformation("User {Email} registered successfully with role {Role}", registerDto.Email, role);

        await _accountService.RegisterAccountAsync(registerDto, role);
    }

    public async Task<AuthResponse> LoginAsync(AccountLoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);
        
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed - User not found: {Email}", loginDto.Email);
            throw new Exception("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed - Invalid password for user: {Email}", loginDto.Email);
            throw new Exception("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        AuthResponse authResponse = await _tokenService.GenerateAuthTokenAsync(user.Email ?? throw new InvalidOperationException("User email cannot be null"), roles);
        _logger.LogInformation("Login successful for user: {Email}", loginDto.Email);
        return authResponse;
    }

public async Task<AuthResponse> RefreshTokenAsync(string? refreshToken = null)
{
    try
    {
        var principal = await _tokenService.GetPrincipalFromExpiredToken(refreshToken ?? throw new Exception("Refresh token not found"));
        
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
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

        return await _tokenService.GenerateAuthTokenAsync(email, roles);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error refreshing token for user");
        throw;
        }
    }

    public async Task RegisterUserAsync(AccountRegisterDto registerDto)
    {
        await RegisterUserWithRoleAsync(registerDto, "User");
    }

    public async Task RegisterAdminAsync(AccountRegisterDto registerDto)
    {
        await RegisterUserWithRoleAsync(registerDto, "Admin");
    }
} 
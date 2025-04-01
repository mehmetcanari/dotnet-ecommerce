using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.Services.Token;

namespace OnlineStoreWeb.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> LoginAsync(AccountLoginDto loginDto)
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
        var token = _tokenService.GenerateToken(user.Email ?? throw new InvalidOperationException("User email cannot be null"), roles);

        _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
        return token;
    }

    public async Task RegisterUserAsync(AccountRegisterDto registerDto)
    {
        await RegisterUserWithRoleAsync(registerDto, "User");
    }

    public async Task RegisterAdminAsync(AccountRegisterDto registerDto)
    {
        await RegisterUserWithRoleAsync(registerDto, "Admin");
    }

    public async Task LogoutAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    private async Task RegisterUserWithRoleAsync(AccountRegisterDto registerDto, string roleName)
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
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to create role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                throw new Exception($"Error creating {roleName} role.");
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addRoleResult.Succeeded)
        {
            _logger.LogError("Failed to add role to user: {Errors}", string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            throw new Exception("Error assigning role to user.");
        }

        _logger.LogInformation("User {Email} registered successfully with role {Role}", registerDto.Email, roleName);
    }
} 
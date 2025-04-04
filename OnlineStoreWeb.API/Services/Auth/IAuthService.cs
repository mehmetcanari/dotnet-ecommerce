using Microsoft.AspNetCore.Identity;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Auth;

namespace OnlineStoreWeb.API.Services.Auth;

public interface IAuthService
{
    Task RegisterUserAsync(AccountRegisterDto registerDto);
    Task RegisterAdminAsync(AccountRegisterDto registerDto);
    Task<AuthResponse> LoginAsync(AccountLoginDto loginDto);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
} 
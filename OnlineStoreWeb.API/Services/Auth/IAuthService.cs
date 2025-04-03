using Microsoft.AspNetCore.Identity;
using OnlineStoreWeb.API.DTO.Request.Account;

namespace OnlineStoreWeb.API.Services.Auth;

public interface IAuthService
{
    Task<string> LoginAsync(AccountLoginDto loginDto);
    Task RegisterUserAsync(AccountRegisterDto registerDto);
    Task RegisterAdminAsync(AccountRegisterDto registerDto);
} 
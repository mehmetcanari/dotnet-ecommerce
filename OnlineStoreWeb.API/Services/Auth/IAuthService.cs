using Microsoft.AspNetCore.Identity;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Auth;

namespace OnlineStoreWeb.API.Services.Auth;

public interface IAuthService
{
    Task RegisterUserAsync(AccountRegisterRequestDto registerRequestDto);
    Task RegisterAdminAsync(AccountRegisterRequestDto registerRequestDto);
    Task<AuthResponseDto> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
} 
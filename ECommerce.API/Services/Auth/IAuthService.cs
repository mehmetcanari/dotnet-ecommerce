using ECommerce.API.DTO.Request.Account;
using ECommerce.API.DTO.Response.Auth;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.API.Services.Auth;

public interface IAuthService
{
    Task RegisterUserAsync(AccountRegisterRequestDto registerRequestDto);
    Task RegisterAdminAsync(AccountRegisterRequestDto registerRequestDto);
    Task<AuthResponseDto> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
} 
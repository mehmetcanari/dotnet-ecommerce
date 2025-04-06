using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;

namespace ECommerce.Application.Services.Auth;

public interface IAuthService
{
    Task RegisterUserAsync(AccountRegisterRequestDto registerRequestDto);
    Task RegisterAdminAsync(AccountRegisterRequestDto registerRequestDto);
    Task<AuthResponseDto> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
} 
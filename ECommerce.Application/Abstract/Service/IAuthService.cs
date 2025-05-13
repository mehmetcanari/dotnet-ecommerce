using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task<AuthResponseDto> GenerateAuthTokenAsync(RefreshToken refreshToken);
    Task RegisterUserWithRoleAsync(AccountRegisterRequestDto registerRequestDto, string role);
} 
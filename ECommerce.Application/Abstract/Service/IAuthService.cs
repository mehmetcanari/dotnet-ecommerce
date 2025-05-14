using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task<Result<AuthResponseDto>> GenerateAuthTokenAsync(RefreshToken refreshToken);
    Task<Result> RegisterUserWithRoleAsync(AccountRegisterRequestDto registerRequestDto, string role);
} 
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(AccountLoginRequestDto loginRequestDto);
    Task <Result> LogoutAsync(string reason);
    Task<Result<AuthResponseDto>> GenerateAuthTokenAsync();
    Task<Result> RegisterAsync(AccountRegisterRequestDto registerRequestDto, string role);
} 
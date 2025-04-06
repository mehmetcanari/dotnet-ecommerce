using System.Security.Claims;
using ECommerce.Application.DTO.Response.Auth;

namespace ECommerce.Application.Services.Token;

public interface ITokenService
{
    Task<string> GenerateToken(string email, IList<string> roles);
    Task<AuthResponseDto> GenerateAuthTokenAsync(string email, IList<string> roles);
    Task<string> GenerateRefreshToken(string email);
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
} 
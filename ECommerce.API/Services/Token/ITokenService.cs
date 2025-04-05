using System.Security.Claims;
using ECommerce.API.DTO.Response.Auth;

namespace ECommerce.API.Services.Token;

public interface ITokenService
{
    Task<string> GenerateToken(string email, IList<string> roles);
    Task<AuthResponseDto> GenerateAuthTokenAsync(string email, IList<string> roles);
    Task<string> GenerateRefreshToken(string email);
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
} 
using System.Security.Claims;
using OnlineStoreWeb.API.DTO.Response.Auth;

namespace OnlineStoreWeb.API.Services.Token;

public interface ITokenService
{
    Task<string> GenerateToken(string email, IList<string> roles);
    Task<AuthResponse> GenerateAuthTokenAsync(string email, IList<string> roles);
    Task<string> GenerateRefreshToken(string email);
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
} 
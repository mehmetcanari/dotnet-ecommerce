using System.Security.Claims;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Interfaces.Service;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string email, IList<string> roles);
    Task<(string, IList<string>)> ValidateRefreshToken(ClaimsPrincipal principal, UserManager<IdentityUser> userManager);
    Task RevokeUserTokens(string email, string reason);
    Task<RefreshToken> GetRefreshTokenFromCookie();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
    void DeleteRefreshTokenCookie();
}

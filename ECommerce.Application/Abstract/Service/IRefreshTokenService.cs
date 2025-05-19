using System.Security.Claims;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> GenerateRefreshTokenAsync(string email, IList<string> roles);
    Task<(string, IList<string>)> ValidateRefreshToken(ClaimsPrincipal principal, UserManager<IdentityUser> userManager);
    Task<Result> RevokeUserTokens(string email, string reason);
    Task<Result> LogoutUserRefreshToken(string reason);
    Task<Result<RefreshToken>> GetRefreshTokenFromCookie();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
}

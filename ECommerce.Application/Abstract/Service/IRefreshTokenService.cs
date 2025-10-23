using System.Security.Claims;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Identity;
using ECommerce.Application.Utility;
using ECommerce.Application.DTO.Request.Token;
namespace ECommerce.Application.Abstract.Service;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> GenerateRefreshTokenAsync(string userId, string email, IList<string> roles);
    Task<Result<(string, IList<string>)>> ValidateRefreshToken(ClaimsPrincipal principal, UserManager<User> userManager);
    Task<Result> RevokeUserTokens(TokenRevokeRequestDto request);
    Task<Result<RefreshToken>> GetRefreshTokenFromCookie();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
}

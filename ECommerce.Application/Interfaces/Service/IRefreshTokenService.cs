using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string email, IList<string> roles);
    Task RevokeUserTokensAsync(string email, string reason);
    Task CleanupExpiredTokensAsync();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenFromCookie();
    void DeleteRefreshTokenCookie();
}

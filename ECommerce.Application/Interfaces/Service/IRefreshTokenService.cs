using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string email, IList<string> roles);
    Task<bool> RevokeRefreshTokenAsync(string token, string? reason = null);
    Task<bool> RevokeAllUserTokensAsync(string email, string? reason = null);
    Task CleanupExpiredTokensAsync();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenFromCookie();
}

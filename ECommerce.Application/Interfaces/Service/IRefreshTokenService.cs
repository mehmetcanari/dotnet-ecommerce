using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string email, IList<string> roles);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<bool> RevokeAllUserTokensAsync(string email);
    Task CleanupExpiredTokensAsync();
    void SetRefreshTokenCookie(RefreshToken refreshToken);
    Task<string> GetRefreshTokenFromCookie();
}

using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetUserTokensAsync(string email);
    Task CreateAsync(RefreshToken refreshToken);
    void Revoke(RefreshToken refreshToken, string? reason = null);
    Task CleanupExpiredTokensAsync();
}
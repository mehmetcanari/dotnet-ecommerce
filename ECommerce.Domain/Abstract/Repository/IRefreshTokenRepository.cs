using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetActiveUserTokenAsync(string email, CancellationToken cancellationToken = default);
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Revoke(RefreshToken refreshToken, string? reason = null);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
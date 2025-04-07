using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetUserTokensAsync(string userId);
        Task CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RevokeAsync(string token);
        Task RevokeAllUserTokensAsync(string userId);
        Task CleanupExpiredTokensAsync();
    }
}

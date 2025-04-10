using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetUserTokensAsync(string email);
        Task<RefreshToken> GetUserTokenAsync(string email);
        Task CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RevokeAsync(RefreshToken refreshToken, string? reason = null);
        Task CleanupExpiredTokensAsync();
    }
}

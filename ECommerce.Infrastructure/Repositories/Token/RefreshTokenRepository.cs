using ECommerce.Application.Interfaces.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.Token
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly StoreDbContext _context;

        public RefreshTokenRepository(StoreDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to create refresh token", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }

        public async Task<RefreshToken> GetUserTokenAsync(string email)
        {
            try
            {
                return await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Email == email)
                    ?? throw new Exception("Refresh token not found");
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to get user token", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetUserTokensAsync(string email)
        {
            try
            {
                return await _context.RefreshTokens
                    .Where(rt => rt.Email == email)
                    .ToListAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to get user tokens", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            try
            {
                return await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token && rt.Expires > DateTime.UtcNow)
                    ?? throw new Exception("Refresh token not found");
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to get refresh token", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get refresh token", ex);
            }
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        { 
            try
            {
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to update refresh token", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }

        public async Task RevokeAsync(RefreshToken refreshToken, string? reason = null)
        {
            try
            {
                refreshToken.RevokeToken(reason);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to revoke token", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.RefreshTokens
                    .Where(rt => rt.IsExpired)
                    .ToListAsync();

                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to cleanup expired tokens", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred", ex);
            }
        }
    }
}

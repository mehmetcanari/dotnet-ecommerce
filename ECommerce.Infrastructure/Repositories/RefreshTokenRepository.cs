using ECommerce.Domain.Model;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;

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
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating refresh token", exception);
        }
    }
    
    public async Task<RefreshToken?> GetActiveUserTokenAsync(string email)
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .Where(rt => rt.Email == email && rt.Expires > DateTime.UtcNow && rt.Revoked == null)
                .FirstOrDefaultAsync();
            
            return refreshToken;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting active user token", exception);
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.Expires > DateTime.UtcNow && rt.Revoked == null);

            return refreshToken;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public void Revoke(RefreshToken refreshToken, string? reason = null)
    {
        try
        {
            refreshToken.RevokeToken(reason);
            _context.RefreshTokens.Update(refreshToken);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while revoking token", exception);
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            var expiredTokens = await query
                .AsNoTracking()
                .Where(rt => rt.Expires < DateTime.UtcNow || rt.Revoked != null)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while cleaning up expired tokens", exception);
        }
    }
}


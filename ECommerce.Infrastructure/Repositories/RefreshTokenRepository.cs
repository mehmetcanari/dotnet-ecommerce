using ECommerce.Application.Interfaces.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

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
                .AsNoTracking()
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
                .AsNoTracking()
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
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsExpired == false)
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

    public void Update(RefreshToken refreshToken)
    {
        try
        {
            _context.RefreshTokens.Update(refreshToken);
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

    public void Revoke(RefreshToken refreshToken, string? reason = null)
    {
        try
        {
            refreshToken.RevokeToken(reason);
            _context.RefreshTokens.Update(refreshToken);
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
                .AsNoTracking()
                .Where(rt => rt.IsExpired)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
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


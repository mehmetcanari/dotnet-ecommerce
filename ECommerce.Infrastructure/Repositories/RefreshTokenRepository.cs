using ECommerce.Application.Interfaces.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public RefreshTokenRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        try
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<RefreshToken> GetUserTokenAsync(string email)
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Email == email);

            if (refreshToken == null)
            {
                throw new Exception("Refresh token not found");
            }

            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<IEnumerable<RefreshToken>> GetUserTokensAsync(string email)
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            return await query
                .AsNoTracking()
                .Where(rt => rt.Email == email)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<RefreshToken> GetByTokenAsync(string token)
    {
        try
        {
            IQueryable<RefreshToken> query = _context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsExpired == false);

            if (refreshToken == null)
            {
                throw new Exception("Refresh token not found");
            }

            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public void Update(RefreshToken refreshToken)
    {
        try
        {
            _context.RefreshTokens.Update(refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}


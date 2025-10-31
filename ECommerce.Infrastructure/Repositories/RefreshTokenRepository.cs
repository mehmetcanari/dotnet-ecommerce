using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using DbContext = ECommerce.Infrastructure.Context.DbContext;

namespace ECommerce.Infrastructure.Repositories;

public class RefreshTokenRepository(DbContext context) : IRefreshTokenRepository
{
    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<RefreshToken?> GetActive(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<RefreshToken> query = context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .Where(rt => rt.Email == email && rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null)
                .FirstOrDefaultAsync(cancellationToken);
            
            return refreshToken;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<RefreshToken> query = context.RefreshTokens;

            var refreshToken = await query
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null, cancellationToken);

            return refreshToken;
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.UnexpectedError, ex);
        }
    }

    public void Revoke(RefreshToken refreshToken, string? reason = null)
    {
        try
        {
            refreshToken.RevokeToken(reason);
            context.RefreshTokens.Update(refreshToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<RefreshToken> query = context.RefreshTokens;

            var expiredTokens = await query
                .AsNoTracking()
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt != null)
                .ToListAsync(cancellationToken);

            context.RefreshTokens.RemoveRange(expiredTokens);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}


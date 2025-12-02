using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(MongoDbContext context)
    {
        _refreshTokens = context.GetCollection<RefreshToken>("refresh_tokens");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var tokenIndexKeys = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token);
        _refreshTokens.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(tokenIndexKeys));

        var ttlIndexKeys = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.ExpiresAt);
        var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }; 

        _refreshTokens.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(ttlIndexKeys, ttlOptions));
    }

    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshTokens.InsertOneAsync(refreshToken, new InsertOneOptions(), cancellationToken);
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
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.Email, email),
                Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, DateTime.UtcNow),
                Builders<RefreshToken>.Filter.Eq(rt => rt.RevokedAt, null)
            );

            var sort = Builders<RefreshToken>.Sort.Descending(rt => rt.CreatedOn);

            return await _refreshTokens.Find(filter).Sort(sort).FirstOrDefaultAsync(cancellationToken);
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
            var filter = Builders<RefreshToken>.Filter.And(
                Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token),
                Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, DateTime.UtcNow),
                Builders<RefreshToken>.Filter.Eq(rt => rt.RevokedAt, null)
            );

            return await _refreshTokens.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.UnexpectedError, ex);
        }
    }

    public async Task Revoke(RefreshToken refreshToken, string? reason = null)
    {
        try
        {
            refreshToken.RevokeToken(reason);
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Id, refreshToken.Id);

            await _refreshTokens.ReplaceOneAsync(filter, refreshToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
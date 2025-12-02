using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public sealed class WishlistRepository : IWishlistRepository
{
    private readonly IMongoCollection<WishlistItem> _wishlistItems;

    public WishlistRepository(MongoDbContext context)
    {
        _wishlistItems = context.GetCollection<WishlistItem>("wishlist_items");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userIndexKeys = Builders<WishlistItem>.IndexKeys.Ascending(w => w.UserId);

        var uniqueItemIndexKeys = Builders<WishlistItem>.IndexKeys
            .Ascending(w => w.UserId)
            .Ascending(w => w.ProductId);

        var indexOptions = new CreateIndexOptions { Unique = true };

        _wishlistItems.Indexes.CreateOneAsync(new CreateIndexModel<WishlistItem>(userIndexKeys));
        _wishlistItems.Indexes.CreateOneAsync(new CreateIndexModel<WishlistItem>(uniqueItemIndexKeys, indexOptions));
    }

    public async Task Create(WishlistItem item, CancellationToken cancellationToken)
    {
        try
        {
            await _wishlistItems.InsertOneAsync(item, new InsertOneOptions(), cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new Exception(ErrorMessages.ProductExists, ex);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<List<WishlistItem>> Read(Guid userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new FindOptions<WishlistItem>
            {
                Skip = (page - 1) * pageSize,
                Limit = pageSize,
                Sort = Builders<WishlistItem>.Sort.Descending(w => w.CreatedOn)
            };

            var cursor = await _wishlistItems.FindAsync(w => w.UserId == userId, options, cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<WishlistItem?> GetById(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<WishlistItem>.Filter.And(
                Builders<WishlistItem>.Filter.Eq(w => w.UserId, userId),
                Builders<WishlistItem>.Filter.Eq(w => w.ProductId, productId)
            );

            var cursor = await _wishlistItems.FindAsync(filter, cancellationToken: cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Delete(WishlistItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            await _wishlistItems.DeleteOneAsync(w => w.Id == item.Id, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<bool> Exists(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<WishlistItem>.Filter.And(
                Builders<WishlistItem>.Filter.Eq(w => w.UserId, userId),
                Builders<WishlistItem>.Filter.Eq(w => w.ProductId, productId)
            );

            var count = await _wishlistItems.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Enum;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _notifications;

    public NotificationRepository(MongoDbContext context)
    {
        _notifications = context.GetCollection<Notification>("notifications");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<Notification>.IndexKeys
            .Ascending(n => n.UserId)
            .Ascending(n => n.Status)     
            .Descending(n => n.CreatedOn); 

        _notifications.Indexes.CreateOneAsync(new CreateIndexModel<Notification>(indexKeys));
    }

    public async Task CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _notifications.InsertOneAsync(notification, new InsertOneOptions(), cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<IEnumerable<Notification>> GetAsync(Guid userId, int page = 1, int size = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);

            var options = new FindOptions<Notification>
            {
                Skip = (page - 1) * size,
                Limit = size,
                Sort = Builders<Notification>.Sort.Descending(n => n.CreatedOn)
            };

            var cursor = await _notifications.FindAsync(filter, options, cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.Status, NotificationStatus.Unread)
            );

            var sort = Builders<Notification>.Sort.Descending(n => n.CreatedOn);

            return await _notifications.Find(filter).Sort(sort).ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.Status, NotificationStatus.Unread)
            );

            var count = await _notifications.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return (int)count;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _notifications.DeleteOneAsync(n => n.Id == id, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);

            var update = Builders<Notification>.Update
                .Set(n => n.Status, NotificationStatus.Read)
                .Set(n => n.UpdatedOn, DateTime.UtcNow); 

            var result = await _notifications.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            return result.ModifiedCount > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.Status, NotificationStatus.Unread)
            );

            var update = Builders<Notification>.Update
                .Set(n => n.Status, NotificationStatus.Read)
                .Set(n => n.UpdatedOn, DateTime.UtcNow);

            var result = await _notifications.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);

            return result.ModifiedCount > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using DbContext = ECommerce.Infrastructure.Context.DbContext;

namespace ECommerce.Infrastructure.Repositories;

public class NotificationRepository(DbContext context) : INotificationRepository
{
    public async Task CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Notifications.AddAsync(notification, cancellationToken);
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
            IQueryable<Notification> query = context.Notifications;

            var notifications = await query
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedOn)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            return notifications;
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
            IQueryable<Notification> query = context.Notifications;

            var notifications = await query
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync(cancellationToken);

            return notifications;
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
            IQueryable<Notification> query = context.Notifications;

            var count = await query
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread, cancellationToken);

            return count;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public void Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = context.Notifications.Find(id);
            if (notification is not null)
                context.Notifications.Remove(notification);

            return;
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
            var notification = await context.Notifications.FindAsync(id, cancellationToken);
            if (notification == null) return false;
            
            notification.MarkAsRead();
            context.Notifications.Update(notification);
            return true;
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
            IQueryable<Notification> query = context.Notifications;

            var unreadNotifications = await query
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync(cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
                context.Notifications.Update(notification);
            }

            return true;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
} 
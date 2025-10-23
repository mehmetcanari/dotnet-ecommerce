using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly StoreDbContext _context;

    public NotificationRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating notification", exception);
        }
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int size = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Notification> query = _context.Notifications;

            var notifications = await query
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            return notifications;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting user notifications", exception);
        }
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Notification> query = _context.Notifications;

            var notifications = await query
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

            return notifications;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting unread notifications", exception);
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Notification> query = _context.Notifications;

            var count = await query
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread, cancellationToken);

            return count;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting unread count", exception);
        }
    }

    public void UpdateAsync(Notification notification)
    {
        try
        {
            _context.Notifications.Update(notification);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating notification", exception);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id, cancellationToken);
            if (notification == null) return false;
            
            _context.Notifications.Remove(notification);
            return true;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting notification", exception);
        }
    }

    public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id, cancellationToken);
            if (notification == null) return false;
            
            notification.MarkAsRead();
            _context.Notifications.Update(notification);
            return true;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while marking notification as read", exception);
        }
    }

    public async Task<bool> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Notification> query = _context.Notifications;
            var unreadNotifications = await query
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync(cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
                _context.Notifications.Update(notification);
            }

            return true;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while marking all notifications as read", exception);
        }
    }

    public async Task<bool> DeleteOldNotificationsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldNotifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            _context.Notifications.RemoveRange(oldNotifications);
            return true;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting old notifications", exception);
        }
    }
} 
using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface INotificationRepository
{
    Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int accountId, int page = 1, int size = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int accountId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    void UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteOldNotificationsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default);
}
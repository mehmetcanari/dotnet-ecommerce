using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface INotificationRepository
{
    Task CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int accountId, int page = 1, int size = 50);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int accountId);
    Task<int> GetUnreadCountAsync(string userId);
    void UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAllAsReadAsync(int id);
    Task<bool> DeleteOldNotificationsAsync(int daysToKeep = 30);
}
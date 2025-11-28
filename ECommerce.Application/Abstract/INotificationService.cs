using ECommerce.Domain.Model;
using ECommerce.Shared.Wrappers;

namespace ECommerce.Application.Abstract;

public interface INotificationService
{
    Task<Result<Notification>> CreateNotificationAsync(string title, string message, NotificationType type);
    Task<Result<IEnumerable<Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50);
    Task<Result<IEnumerable<Notification>>> GetUnreadNotificationsAsync();
    Task<Result<int>> GetUnreadNotificationsCountAsync();
    Task<Result<bool>> MarkAsReadAsync(Guid notificationId);
    Task<Result<bool>> MarkAllAsReadAsync();
    Task<Result> DeleteNotificationAsync(Guid notificationId);
}
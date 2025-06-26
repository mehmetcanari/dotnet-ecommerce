using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface INotificationService
{
    Task<Result<Notification>> CreateNotificationAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null);
    Task<Result<bool>> SendNotificationToUserAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null);
    Task<Result<IEnumerable<Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50);
    Task<Result<IEnumerable<Notification>>> GetUnreadNotificationsAsync();
    Task<Result<int>> GetUnreadCountAsync();
    Task<Result<bool>> MarkAsReadAsync(int notificationId);
    Task<Result<bool>> MarkAllAsReadAsync();
    Task<Result<bool>> DeleteNotificationAsync(int notificationId);
} 
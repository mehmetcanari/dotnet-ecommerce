using ECommerce.Application.DTO.Request.Notification;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface INotificationService
{
    Task<Result<Notification>> TestCreateNotificationAsync(SendNotificationRequestDto request);
    Task<Result<Notification>> CreateNotificationAsync(string title, string message, NotificationType type);
    Task<Result<IEnumerable<Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50);
    Task<Result<IEnumerable<Notification>>> GetUnreadNotificationsAsync();
    Task<Result<int>> GetUnreadNotificationsCountAsync();
    Task<Result<bool>> MarkAsReadAsync(Guid notificationId);
    Task<Result<bool>> MarkAllAsReadAsync();
    Task<Result<bool>> DeleteNotificationAsync(Guid notificationId);
} 
using ECommerce.Shared.Enum;

namespace ECommerce.Application.Abstract;

public interface IRealtimeNotificationHandler
{
    Task HandleNotification(string title, string message, NotificationType type, Guid userId);
}
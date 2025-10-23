using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface IRealtimeNotificationHandler
{
    Task HandleNotification(string title, string message, NotificationType type, string userId);
}
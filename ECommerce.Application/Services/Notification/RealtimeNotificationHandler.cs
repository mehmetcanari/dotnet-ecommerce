using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Notification;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

public class RealtimeNotificationHandler : IRealtimeNotificationHandler
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealtimeNotificationHandler(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task HandleNotification(string title, string message, NotificationType type, Guid userId)
    {
        var notification = new Domain.Model.Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Status = NotificationStatus.Unread
        };

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", MapToResponseDto(notification));
    }

    private static NotificationResponseDto MapToResponseDto(Domain.Model.Notification notification) => new NotificationResponseDto
    {
        Title = notification.Title,
        Message = notification.Message,
        From = notification.Type.ToString()
    };
}
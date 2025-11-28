using ECommerce.Application.Abstract;
using ECommerce.Shared.DTO.Response.Notification;
using ECommerce.Shared.Enum;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

public class RealtimeNotificationHandler(IHubContext<NotificationHub> hubContext) : IRealtimeNotificationHandler
{
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

        await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", MapToResponseDto(notification));
    }

    private static NotificationResponseDto MapToResponseDto(Domain.Model.Notification notification) => new NotificationResponseDto
    {
        Title = notification.Title,
        Message = notification.Message,
        From = notification.Type.ToString()
    };
}
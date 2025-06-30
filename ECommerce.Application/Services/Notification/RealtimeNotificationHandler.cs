using ECommerce.Application.Abstract.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

public class RealtimeNotificationHandler : IRealtimeNotificationHandler
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICurrentUserService _currentUserService;

    public RealtimeNotificationHandler(IHubContext<NotificationHub> hubContext, ICurrentUserService currentUserService)
    {
        _hubContext = hubContext;
        _currentUserService = currentUserService;
    }
    public async Task HandleNotification(string title, string message, NotificationType type, string? userId = null)
    {
        if (string.IsNullOrEmpty(userId))
        {
            var userIdResult = await _currentUserService.GetCurrentUserId();
            if (userIdResult.IsSuccess && !string.IsNullOrEmpty(userIdResult.Data))
            {
                userId = userIdResult.Data;
            }
        }

        var notification = new Domain.Model.Notification
        {
            AccountId = int.Parse(userId!),
            Title = title,
            Message = message,
            Type = type,
            Status = NotificationStatus.Unread,
            CreatedAt = DateTime.UtcNow,
        };

        await _hubContext.Clients.User(userId!).SendAsync("ReceiveNotification", notification);
    }
}
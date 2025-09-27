using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Notification;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

public class RealtimeNotificationHandler : IRealtimeNotificationHandler
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;

    public RealtimeNotificationHandler(IHubContext<NotificationHub> hubContext, ICurrentUserService currentUserService, ILoggingService logger)
    {
        _hubContext = hubContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    public async Task HandleNotification(string title, string message, NotificationType type, Guid? userId = null, int? accountId = null)
    {
        if (userId == null)
        {
            var userIdResult = await _currentUserService.GetUserId();
            if (userIdResult.IsSuccess && !string.IsNullOrEmpty(userIdResult.Data))
            {
                userId = Guid.Parse(userIdResult.Data);
            }
        }

        var notification = new Domain.Model.Notification
        {
            AccountId = accountId,
            Title = title,
            Message = message,
            Type = type,
            Status = NotificationStatus.Unread,
            CreatedAt = DateTime.UtcNow,
        };

        await _hubContext.Clients.User(userId!.Value.ToString()).SendAsync("ReceiveNotification", MapToResponseDto(notification));
        _logger.LogInformation("Realtime notification sent to user with id {UserId}", userId);
    }

    private static NotificationResponseDto MapToResponseDto(Domain.Model.Notification notification)
    {
        return new NotificationResponseDto
        {
            Title = notification.Title,
            Message = notification.Message,
            From = notification.Type.ToString()
        };
    }
}
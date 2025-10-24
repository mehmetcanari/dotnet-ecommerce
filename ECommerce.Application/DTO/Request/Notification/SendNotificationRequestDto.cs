using ECommerce.Domain.Model;

namespace ECommerce.Application.DTO.Request.Notification;

public class SendNotificationRequestDto
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public NotificationType Type { get; set; }
}
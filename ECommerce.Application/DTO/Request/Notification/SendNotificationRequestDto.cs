using ECommerce.Domain.Model;

namespace ECommerce.Application.DTO.Request.Notification;

public class SendNotificationRequestDto
{
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Domain.Model;

public class Notification : BaseEntity
{
    [StringLength(50)] public required string Title { get; init; }
    [StringLength(200)] public required string Message { get; init; }
    public NotificationType Type { get; init; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime? ReadAt { get; set; }

    // Navigation property
    public User? User { get; set; }
    public Guid UserId { get; set; }

    
    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read)
            return;
            
        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
    }
    
    public void MarkAsUnread()
    {
        Status = NotificationStatus.Unread;
        ReadAt = null;
    }
}
namespace ECommerce.Domain.Model;

public class Notification : BaseEntity
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public NotificationType Type { get; set; }
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
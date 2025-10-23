namespace ECommerce.Domain.Model;

public class Notification
{
    public int Id { get; init; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation property
    public User? User { get; set; }
    public int? AccountId { get; set; }

    
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

public enum NotificationType
{
    OrderStatus = 1,
    Payment = 2,
    Stock = 3,
    Promotion = 4,
    System = 5
}

public enum NotificationStatus
{
    Unread = 1,
    Read = 2
} 
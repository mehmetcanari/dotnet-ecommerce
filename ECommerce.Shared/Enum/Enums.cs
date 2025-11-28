namespace ECommerce.Shared.Enum;

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

public enum OrderStatus
{
    Pending = 0,
    Shipped = 1,
    Delivered = 2,
    Cancelled = 3
}

public enum CacheExpirationType
{
    Absolute,
    Sliding
}
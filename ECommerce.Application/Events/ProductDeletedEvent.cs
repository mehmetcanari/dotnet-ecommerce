using MediatR;

namespace ECommerce.Application.Events;

public class ProductDeletedEvent : INotification
{
    public int ProductId { get; init; }
    public required string ProductName { get; init; }
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
} 
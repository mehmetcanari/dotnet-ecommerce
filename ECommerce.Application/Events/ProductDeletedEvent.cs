using MediatR;

namespace ECommerce.Application.Events;

public class ProductDeletedEvent : INotification
{
    public required Guid Id { get; init; }
    public required string ProductName { get; init; }
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
} 
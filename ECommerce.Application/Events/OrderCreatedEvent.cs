namespace ECommerce.Application.Events;

public class OrderCreatedEvent
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public string ShippingAddress { get; init; } = string.Empty;
    public string BillingAddress { get; init; } = string.Empty;
    public OrderStatus Status { get; init; } = OrderStatus.Pending;
}
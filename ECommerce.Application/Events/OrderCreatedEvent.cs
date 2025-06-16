public class OrderCreatedEvent
{
    public int OrderId { get; init; }
    public int AccountId { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public string ShippingAddress { get; init; } = string.Empty;
    public string BillingAddress { get; init; } = string.Empty;
    public OrderStatus Status { get; init; } = OrderStatus.Pending;
}
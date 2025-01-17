public record CreateOrderDto
{
    public OrderItem OrderItem { get; set; }
    public DateTime OrderCreated = DateTime.UtcNow;
    public DateTime OrderUpdated = DateTime.UtcNow;
}
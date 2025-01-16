public record CreateOrderDto
{
    public OrderItem OrderItem { get; set; }
    public DateTime OrderCreated = DateTime.UtcNow;
}
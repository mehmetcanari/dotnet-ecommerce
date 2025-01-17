public record UpdateOrderDto
{
    public OrderItem OrderItem { get; set; }
    public DateTime OrderUpdated = DateTime.UtcNow;
}
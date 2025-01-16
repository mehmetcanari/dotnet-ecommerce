public record UpdateOrderDto
{
    public int Id { get; set; }
    public OrderItem OrderItem { get; set; }
    public DateTime OrderUpdated = DateTime.UtcNow;
}
public record UpdateOrderItemDto
{
    public int Id { get; set; }
    public int Quantity {get;set;}
    public Product Product { get; set; }
    public DateTime OrderItemUpdated = DateTime.UtcNow;
}
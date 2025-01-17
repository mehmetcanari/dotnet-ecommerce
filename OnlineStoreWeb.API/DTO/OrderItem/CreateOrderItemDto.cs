public record CreateOrderItemDto
{
    public int Quantity {get;set;}
    public Product Product { get; set; }
    public DateTime OrderItemCreated = DateTime.UtcNow;
    public DateTime OrderItemUpdated = DateTime.UtcNow;
}
public record UpdateOrderItemDto
{
    public int Quantity {get;set;}
    public Product Product { get; set; }
    public DateTime OrderItemUpdated = DateTime.Now;
}
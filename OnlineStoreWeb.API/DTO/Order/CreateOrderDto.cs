public record CreateOrderDto
{
    public Product Product { get; set; }
    public DateTime OrderCreated = DateTime.Now;
}
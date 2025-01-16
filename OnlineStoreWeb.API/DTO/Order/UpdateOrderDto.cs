public record UpdateOrderDto
{
    public Product Product { get; set; }
    public DateTime OrderUpdated = DateTime.Now;
}
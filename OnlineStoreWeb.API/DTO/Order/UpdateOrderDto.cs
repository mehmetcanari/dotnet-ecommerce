public record UpdateOrderDto
{
    public int Id { get; set; }
    public Product Product { get; set; }
    public DateTime OrderUpdated = DateTime.Now;
}
public record CreateCartDto
{
    public int Id { get; set; }
    public double TotalPrice {get; set;}
    public List<OrderItem> OrderItems {get; set;}
}
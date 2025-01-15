public record UpdateCartDto
{
    public double TotalPrice {get; set;}
    public List<OrderItem> OrderItems {get; set;}
}
public record CreateCartDto
{
    public double TotalPrice {get; set;}
    public List<OrderItem> OrderItems {get; set;}
}
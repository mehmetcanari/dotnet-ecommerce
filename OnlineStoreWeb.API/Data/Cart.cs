public class Cart
{
    public int Id {get; set;}
    public double TotalPrice {get; set;}
    public List<OrderItem> Items { get; set; }
}
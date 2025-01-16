public class OrderItem
{
    public int Id {get; set;}
    public int Quantity {get;set;}
    public Product Product { get; set; }
    public DateTime OrderItemCreated { get; set; }
    public DateTime OrderItemUpdated { get; set; }
}
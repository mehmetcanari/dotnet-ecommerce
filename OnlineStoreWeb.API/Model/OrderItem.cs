public class OrderItem
{
    public int Id {get; set;}
    public int UserId {get;set;}
    public int Quantity {get;set;}
    public int ProductId {get;set;}
    public double Price {get;set;}
    public double TotalPrice 
    {
        get 
        {
        return Price * Quantity;
        }
    }
    public DateTime OrderItemCreated = DateTime.UtcNow;
    public DateTime OrderItemUpdated { get; set; }
}
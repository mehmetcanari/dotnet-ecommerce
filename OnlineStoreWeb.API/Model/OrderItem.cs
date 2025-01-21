public class OrderItem
{
    public int Id {get; set;}
    public required int UserId {get;set;}
    public required int Quantity {get;set;}
    public required int ProductId {get;set;}
    public required double Price {get;set;}
    public double TotalPrice 
    {
        get 
        {
        return Price * Quantity;
        }
    }
    public DateTime OrderItemCreated = DateTime.UtcNow;
    public required DateTime OrderItemUpdated { get; set; }
}
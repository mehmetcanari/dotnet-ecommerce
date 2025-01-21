namespace OnlineStoreWeb.API.Model;

public class OrderItem
{
    public int Id {get; init;}
    public required int UserId {get;init;}
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
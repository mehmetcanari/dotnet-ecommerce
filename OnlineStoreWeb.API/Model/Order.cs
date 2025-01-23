namespace OnlineStoreWeb.API.Model;

public class Order
{
    public int Id { get; init; } 
    public int UserId { get; init; } 
    public int ProductId { get; set; } 
    public double Price { get; init; }
    public int Quantity { get; init; } 
    public double TotalPrice => Price * Quantity;
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public string ShippingAddress { get; set; } 
    public string BillingAddress { get; set; } 
    public PaymentMethod PaymentMethod { get; set; }
    public string AccountName { get; set; }
    public OrderStatus Status { get; internal set; } = OrderStatus.Pending;
}

public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Canceled
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    ApplePay,
}
public class Order
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required string ShippingAddress { get; set; }
    public required string PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    
    public double TotalAmount 
    {
        get
        {
            return OrderItems.Sum(item => item.TotalPrice);
        }
    }
}

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
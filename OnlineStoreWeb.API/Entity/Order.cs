public class Order
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public DateTime OrderDate = DateTime.UtcNow;
    public required string ShippingAddress { get; set; }
    public required string PaymentMethod { get; set; }
    public double TotalAmount 
    {
        get
        {
            return OrderItems.Sum(item => item.TotalPrice);
        }
    }
    public OrderStatus Status { get; set; }
    public List<OrderItem> OrderItems { get; set; }


    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
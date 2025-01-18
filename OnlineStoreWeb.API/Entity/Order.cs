public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate = DateTime.UtcNow;
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
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

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
}
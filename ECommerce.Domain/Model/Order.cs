namespace ECommerce.Domain.Model
{
    public class Order
    {   
        public int OrderId { get; init; }
        public int AccountId { get; init; }
        public ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public decimal TotalPrice => BasketItems.Sum(oi => oi.TotalPrice);
        public DateTime OrderDate { get; init; } = DateTime.UtcNow;
        public required string ShippingAddress { get; init; }
        public required string BillingAddress { get; init; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}


public enum OrderStatus
{
    Pending = 0,
    Shipped = 1,
    Delivered = 2,
    Cancelled = 3
}
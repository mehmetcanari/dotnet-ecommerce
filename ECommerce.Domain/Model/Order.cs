namespace ECommerce.Domain.Model
{
    public class Order : BaseEntity
    {   
        public required Guid UserId { get; init; }
        public ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public decimal TotalPrice => BasketItems.Sum(oi => oi.TotalPrice);
        public required string ShippingAddress { get; init; }
        public required string BillingAddress { get; init; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
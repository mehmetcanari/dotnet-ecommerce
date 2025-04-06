
namespace ECommerce.API.Model
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public required int AccountId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public required int ProductId { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ProductName { get; set; }

        // Navigation properties
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
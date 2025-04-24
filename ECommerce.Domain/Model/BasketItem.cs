
namespace ECommerce.Domain.Model
{
    public class BasketItem
    {
        public int BasketItemId { get; set; }
        public required int AccountId { get; set; }
        public required string ExternalId { get; set; } // This is required for iyzipay, mapping to buyer model
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public required int ProductId { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public required string ProductName { get; set; }
        public bool IsOrdered { get; set; }

        // Navigation properties
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
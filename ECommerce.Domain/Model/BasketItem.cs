
namespace ECommerce.Domain.Model;

public class BasketItem : BaseEntity
{
    public required Guid UserId { get; set; }
    public required Guid ProductId { get; set; }
    public required string ExternalId { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string ProductName { get; set; }
    public bool IsPurchased { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    // Navigation properties
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
}
using MediatR;

namespace ECommerce.Application.Events;

public class ProductStockUpdatedEvent : INotification
{
    public int ProductId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public required string ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime ProductCreated { get; set; }
    public DateTime ProductUpdated { get; set; }
} 
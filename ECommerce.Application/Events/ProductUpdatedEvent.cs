using MediatR;

namespace ECommerce.Application.Events;

public class ProductUpdatedEvent : INotification
{
    public required Guid Id { get; init; }
    public required Guid CategoryId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal Price { get; init; }
    public decimal DiscountRate { get; init; }
    public required string ImageUrl { get; init; }
    public int StockQuantity { get; init; }
}
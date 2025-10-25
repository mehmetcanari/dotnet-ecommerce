namespace ECommerce.Application.DTO.Request.Product;

public record ProductUpdateRequestDto
{
    public required Guid Id { get; set; }
    public required Guid CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required decimal DiscountRate { get; set; }
    public required string? ImageUrl { get; set; }
    public required int StockQuantity { get; set; }
}
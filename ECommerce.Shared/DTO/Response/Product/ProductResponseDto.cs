namespace ECommerce.Shared.DTO.Response.Product;

public record ProductResponseDto
{
    public required Guid Id { get; set; }
    public required Guid CategoryId { get; set; }
    public required string ProductName { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}
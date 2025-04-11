namespace ECommerce.Application.DTO.Response.Product;

public class ProductResponseDto
{
    public required string ProductName { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}
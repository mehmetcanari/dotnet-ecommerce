namespace ECommerce.API.DTO.Response.Product;

public class ProductResponseDto
{
    public string ProductName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}
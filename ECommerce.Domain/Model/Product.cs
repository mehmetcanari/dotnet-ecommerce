using System.Text.Json.Serialization;
namespace ECommerce.Domain.Model;

public class Product
{
    public int ProductId {get; init;}
    [JsonPropertyName("name")]

    public required string Name {get;set;}
    [JsonPropertyName("description")]

    public required string Description {get;set;}
    [JsonPropertyName("price")]

    public required decimal Price {get;set;}
    [JsonPropertyName("discountRate")]

    public required decimal DiscountRate {get;set;}
    [JsonPropertyName("imageUrl")]

    public required string? ImageUrl {get;set;}
    [JsonPropertyName("stockQuantity")]
    
    public required int StockQuantity {get;set;}
    public DateTime ProductCreated { get; init; } = DateTime.UtcNow;
    public DateTime ProductUpdated { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ECommerce.Domain.Model;

public class Product : BaseEntity
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [BsonElement("description")]
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [BsonElement("price")]
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [BsonElement("discountRate")]
    [JsonPropertyName("discountRate")]
    public required decimal DiscountRate { get; set; }

    [BsonElement("imageUrl")]
    [JsonPropertyName("imageUrl")]
    public required string? ImageUrl { get; set; }

    [BsonElement("stockQuantity")]
    [JsonPropertyName("stockQuantity")]
    public required int StockQuantity { get; set; }

    [BsonElement("categoryId")]
    public required Guid CategoryId { get; set; }
}


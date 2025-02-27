namespace OnlineStoreWeb.API.DTO.Request.Product;

public record ProductUpdateDto
{
    public required string Name {get;set;}
    public required string Description {get;set;}
    public required decimal Price {get;set;}
    public required decimal DiscountRate {get;set;}
    public required string? ImageUrl {get;set;}
    public required int StockQuantity {get;set;}
}
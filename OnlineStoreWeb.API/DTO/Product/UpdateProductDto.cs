namespace OnlineStoreWeb.API.DTO.Product;

public record UpdateProductDto
{
    public required string Name {get;set;}
    public required string Description {get;set;}
    public required double Price {get;set;}
    public required string? ImageUrl {get;set;}
    public required int StockQuantity {get;set;}
}
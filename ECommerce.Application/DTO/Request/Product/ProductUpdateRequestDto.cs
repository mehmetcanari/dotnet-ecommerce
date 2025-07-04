namespace ECommerce.Application.DTO.Request.Product;

public record ProductUpdateRequestDto
{
    public required string Name {get;set;}
    public required string Description {get;set;}
    public required double Price {get;set;}
    public required double DiscountRate {get;set;}
    public required string? ImageUrl {get;set;}
    public required int StockQuantity {get;set;}
    public required int CategoryId {get;set;}
}
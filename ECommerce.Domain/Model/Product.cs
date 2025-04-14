namespace ECommerce.Domain.Model;

public class Product
{
    public int ProductId {get; init;}
    public required string Name {get;set;}
    public required string Description {get;set;}
    public required decimal Price {get;set;}
    public required decimal DiscountRate {get;set;}
    public required string? ImageUrl {get;set;}
    public required int StockQuantity {get;set;}
    public DateTime ProductCreated { get; init; } 
    public DateTime ProductUpdated { get; set; }

    //Navigation Properties
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
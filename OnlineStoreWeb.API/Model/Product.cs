public class Product
{
    public int Id {get; set;}
    public required string Name {get;set;}
    public required string Description {get;set;}
    public required double Price {get;set;}
    public string? ImageUrl {get;set;}
    public required int StockQuantity {get;set;}
    public DateTime ProductCreated { get; set; }
    public DateTime ProductUpdated { get; set; }
}
public class Product
{
    public int Id {get; set;}
    public string Name {get;set;}
    public string Description {get;set;}
    public double Price {get;set;}
    public string? ImageUrl {get;set;}
    public int StockQuantity {get;set;}
    public DateTime ProductCreated = DateTime.UtcNow;
    public DateTime ProductUpdated { get; set; }
}
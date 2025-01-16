public record UpdateProductDto
{
    public string Name {get;set;}
    public string Description {get;set;}
    public double Price {get;set;}
    public DateTime ProductUpdated = DateTime.Now;
}
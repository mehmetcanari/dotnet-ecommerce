namespace ECommerce.Domain.Model;

public class Address
{
    public required string ContactName { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string Description { get; set; }
    public required string ZipCode { get; set; }
}

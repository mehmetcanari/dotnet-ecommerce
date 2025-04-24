namespace ECommerce.Domain.Model;
public class Buyer
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Email { get; set; }
    public required string GsmNumber { get; set; }
    public required string IdentityNumber { get; set; }
    public required string RegistrationAddress { get; set; }
    public required string Ip { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string ZipCode { get; set; }
}

namespace ECommerce.Domain.Model;
public class Buyer : BaseEntity
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Email { get; init; }
    public required string GsmNumber { get; init; }
    public required string IdentityNumber { get; init; }
    public required string RegistrationAddress { get; init; }
    public required string Ip { get; init; }
    public required string City { get; init; }
    public required string Country { get; init; }
    public required string ZipCode { get; init; }
}

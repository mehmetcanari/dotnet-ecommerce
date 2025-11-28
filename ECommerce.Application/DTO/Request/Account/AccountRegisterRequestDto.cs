namespace ECommerce.Application.DTO.Request.Account;

public record AccountRegisterRequestDto
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Email { get; set; }
    public required string IdentityNumber { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string ZipCode { get; set; }
    public required string Password { get; set; }
    public required string Address { get; set; }
    public required string Phone { get; set; }
    public required string PhoneCode { get; set; }
    public required bool MembershipAgreement { get; set; }
    public bool PrivacyPolicyConsent { get; set; }
    public bool ElectronicConsent { get; set; }
    public required DateTime DateOfBirth { get; set; }
}
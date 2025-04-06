namespace ECommerce.API.DTO.Request.Account;

public record AccountUpdateRequestDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}
namespace ECommerce.Application.DTO.Response.Account;

public class AccountResponseDto
{
    public int AccountId { get; set; }
    public required string Role { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateTime DateOfBirth { get; init; }
}
namespace ECommerce.Application.DTO.Response.Account;

public class AccountResponseDto
{
    public int AccountId { get; set; }
    public string Role { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; init; }
}
namespace OnlineStoreWeb.API.Model;

public class Account
{
    public int Id { get; init; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateTime DateOfBirth { get; init; }
    public DateTime UserCreated { get; init; } 
    public DateTime? UserUpdated { get; set; }
}
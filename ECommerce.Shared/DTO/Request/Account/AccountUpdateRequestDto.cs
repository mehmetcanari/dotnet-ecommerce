namespace ECommerce.Shared.DTO.Request.Account;

public record AccountUpdateRequestDto
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? ZipCode { get; init; }
    public string? Address { get; init; }
    public string? PhoneCode { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string? OldPassword { get; init; }
}

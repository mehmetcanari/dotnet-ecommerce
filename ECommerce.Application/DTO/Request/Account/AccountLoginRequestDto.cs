namespace ECommerce.Application.DTO.Request.Account;

public record AccountLoginRequestDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
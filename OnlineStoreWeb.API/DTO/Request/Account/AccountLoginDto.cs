namespace OnlineStoreWeb.API.DTO.Request.Account;

public record AccountLoginDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
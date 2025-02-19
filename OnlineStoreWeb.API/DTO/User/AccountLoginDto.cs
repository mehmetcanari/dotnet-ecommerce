namespace OnlineStoreWeb.API.DTO.User;

public record AccountLoginDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
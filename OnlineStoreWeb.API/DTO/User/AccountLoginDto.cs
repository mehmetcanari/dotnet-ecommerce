namespace OnlineStoreWeb.API.DTO.User;

public record AccountLoginDto
{
    public string Email { get; init; }
    public string Password { get; init; }
}
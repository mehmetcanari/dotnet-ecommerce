namespace ECommerce.Shared.DTO.Response.Auth;

public record RefreshTokenResponseDto
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
}
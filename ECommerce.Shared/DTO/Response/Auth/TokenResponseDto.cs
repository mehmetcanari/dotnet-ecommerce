namespace ECommerce.Shared.DTO.Response.Auth;

public record TokenResponseDto
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
}
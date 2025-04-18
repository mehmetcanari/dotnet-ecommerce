namespace ECommerce.Application.DTO.Response.Auth;

public record AuthResponseDto
{
    public required string AccessToken { get; set; }
    public DateTime AccessTokenExpiration { get; set; }
}
namespace ECommerce.Application.DTO.Response.Auth;

public class AuthResponseDto
{
    public required string AccessToken { get; set; }
    public DateTime AccessTokenExpiration { get; set; }
}
namespace ECommerce.Application.DTO.Request.Token;

public record TokenRevokeRequestDto
{
    public required string Email { get; set; }
}
namespace ECommerce.Shared.DTO.Request.Token;

public record TokenRevokeRequestDto
{
    public required string Email { get; init; }
    public required string Reason { get; init; }
}
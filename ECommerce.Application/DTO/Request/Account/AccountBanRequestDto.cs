namespace ECommerce.Application.DTO.Request.Account;

public record AccountBanRequestDto
{
    public required string Email { get; init; }
    public required DateTime Until { get; init; }
    public required string Reason { get; init; }
}
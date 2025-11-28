namespace ECommerce.Shared.DTO.Request.Account;

public record AccountUnbanRequestDto
{
    public required string Email { get; init; }
}
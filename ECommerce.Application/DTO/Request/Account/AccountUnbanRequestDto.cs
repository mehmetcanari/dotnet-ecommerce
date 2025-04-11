namespace ECommerce.Application.DTO.Request.Account;

public record AccountUnbanRequestDto
{
    public required string Email { get; init; }
}
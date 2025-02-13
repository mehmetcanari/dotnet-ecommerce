namespace OnlineStoreWeb.API.DTO.User;

public record AccountPatchDto
{
    public required string Email { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
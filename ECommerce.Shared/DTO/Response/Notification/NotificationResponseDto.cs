namespace ECommerce.Shared.DTO.Response.Notification;

public record NotificationResponseDto
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string From { get; set; }
}
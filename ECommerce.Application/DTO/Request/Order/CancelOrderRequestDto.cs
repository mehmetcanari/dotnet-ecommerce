namespace ECommerce.Application.DTO.Request.Order;

public record OrderCancelRequestDto
{
    public required OrderStatus Status { get; init; }
}
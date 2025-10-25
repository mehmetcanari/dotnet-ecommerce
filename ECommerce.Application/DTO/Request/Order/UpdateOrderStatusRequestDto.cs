namespace ECommerce.Application.DTO.Request.Order;

public record UpdateOrderStatusRequestDto
{
    public required Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
}
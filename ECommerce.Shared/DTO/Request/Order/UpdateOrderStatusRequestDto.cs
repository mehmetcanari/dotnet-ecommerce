using ECommerce.Shared.Enum;

namespace ECommerce.Shared.DTO.Request.Order;

public record UpdateOrderStatusRequestDto
{
    public required Guid UserId { get; set; }
    public required Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
}
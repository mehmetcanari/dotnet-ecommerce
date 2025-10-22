namespace ECommerce.Application.DTO.Request.Order;

public record UpdateOrderStatusRequestDto
{
    public OrderStatus Status { get; set; }
}
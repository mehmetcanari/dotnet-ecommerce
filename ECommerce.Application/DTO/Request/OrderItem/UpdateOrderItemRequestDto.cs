namespace ECommerce.Application.DTO.Request.OrderItem;

public record UpdateOrderItemRequestDto
{
    public required int OrderItemId { get; set; }
    public required int Quantity { get; set; }
    public required int ProductId { get; set; }
}
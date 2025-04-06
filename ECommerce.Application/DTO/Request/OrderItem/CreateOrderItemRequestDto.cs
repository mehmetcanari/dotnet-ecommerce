namespace ECommerce.Application.DTO.Request.OrderItem;

public record CreateOrderItemRequestDto
{
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }
}
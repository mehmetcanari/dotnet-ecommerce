namespace OnlineStoreWeb.API.DTO.Request.OrderItem;

public record CreateOrderItemDto
{
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }
}
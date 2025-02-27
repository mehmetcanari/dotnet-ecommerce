namespace OnlineStoreWeb.API.DTO.Request.OrderItem;

public record UpdateOrderItemDto
{
    public required int AccountId { get; set; }
    public required int OrderItemId { get; set; }
    public required int Quantity { get; set; }
    public required int ProductId { get; set; }
}
namespace OnlineStoreWeb.API.DTO.Request.Order;

public record OrderUpdateDto
{
    public OrderStatus Status { get; set; }
}
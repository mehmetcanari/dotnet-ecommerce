namespace OnlineStoreWeb.API.DTO.Request.Order;

public record OrderUpdateRequestDto
{
    public OrderStatus Status { get; set; }
}
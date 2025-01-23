using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.DTO.Order;

public record UpdateOrderDto
{
    public OrderStatus Status { get; set; }
}
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.DTO.Order;

public record OrderUpdateDto
{
    public OrderStatus Status { get; set; }
}
using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.DTO.Response.OrderItem;

namespace OnlineStoreWeb.API.Services.OrderItem;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync();
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto);
    Task DeleteOrderItemAsync(int orderItemId);
}
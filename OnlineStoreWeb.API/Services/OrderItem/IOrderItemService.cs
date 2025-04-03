using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.DTO.Response.OrderItem;

namespace OnlineStoreWeb.API.Services.OrderItem;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto, string email);
    Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync(string email);
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto, string email);
    Task DeleteAllOrderItemsAsync(string email);
}
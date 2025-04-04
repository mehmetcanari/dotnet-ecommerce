using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.DTO.Response.OrderItem;

namespace OnlineStoreWeb.API.Services.OrderItem;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemRequestDto createOrderItemRequestDto, string email);
    Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync(string email);
    Task UpdateOrderItemAsync(UpdateOrderItemRequestDto updateOrderItemRequestDto, string email);
    Task DeleteAllOrderItemsAsync(string email);
}
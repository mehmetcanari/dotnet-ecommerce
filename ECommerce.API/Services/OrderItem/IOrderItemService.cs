using ECommerce.API.DTO.Request.OrderItem;
using ECommerce.API.DTO.Response.OrderItem;

namespace ECommerce.API.Services.OrderItem;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemRequestDto createOrderItemRequestDto, string email);
    Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync(string email);
    Task UpdateOrderItemAsync(UpdateOrderItemRequestDto updateOrderItemRequestDto, string email);
    Task DeleteAllOrderItemsAsync(string email);
}
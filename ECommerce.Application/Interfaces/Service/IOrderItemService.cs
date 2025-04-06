using ECommerce.Application.DTO.Request.OrderItem;
using ECommerce.Application.DTO.Response.OrderItem;

namespace ECommerce.Application.Interfaces.Service;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemRequestDto createOrderItemRequestDto, string email);
    Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync(string email);
    Task UpdateOrderItemAsync(UpdateOrderItemRequestDto updateOrderItemRequestDto, string email);
    Task DeleteAllOrderItemsAsync(string email);
}
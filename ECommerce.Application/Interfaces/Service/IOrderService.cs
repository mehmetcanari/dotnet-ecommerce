using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;

namespace ECommerce.Application.Services.Order;

public interface IOrderService
{
    Task<List<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto> GetOrdersAsync(string email);
    Task<OrderResponseDto> GetOrderByIdAsync(int id);
    Task AddOrderAsync(OrderCreateRequestDto createRequestOrderDto, string email);
    Task CancelOrderAsync(string email);
    Task DeleteOrderByIdAsync(int id);
    Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto);
}
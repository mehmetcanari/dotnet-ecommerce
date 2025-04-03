using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.DTO.Response.Order;

namespace OnlineStoreWeb.API.Services.Order;

public interface IOrderService
{
    Task<List<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto> GetOrdersAsync(string email);
    Task<OrderResponseDto> GetOrderByIdAsync(int id);
    Task AddOrderAsync(OrderCreateDto createOrderDto, string email);
    Task CancelOrderAsync(string email);
    Task DeleteOrderByIdAsync(int id);
    Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateDto orderUpdateDto);
}
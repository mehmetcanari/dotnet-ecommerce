using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.DTO.Response.Order;

namespace OnlineStoreWeb.API.Services.Order;

public interface IOrderService
{
    Task<List<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto> GetOrderWithIdAsync(int id);
    Task AddOrderAsync(OrderCreateDto createOrderDto);
    Task DeleteOrderByAccountIdAsync(int accountId);
    Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateDto orderUpdateDto);
}
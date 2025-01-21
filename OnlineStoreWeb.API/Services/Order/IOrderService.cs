using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.Services.Order;

public interface IOrderService
{
    Task<List<Model.Order>> GetAllOrdersAsync();
    Task<Model.Order> GetOrderWithIdAsync(int id);
    Task<List<Model.Order>> GetOrdersByUserIdAsync(int userId);
    Task AddOrderAsync(OrderCreateDto createOrderDto);
    Task DeleteOrderAsync(int id);
    Task DeleteOrderWithUserIdAsync(int userId);
    Task UpdateOrderStatusAsync(int id, OrderStatus status);
}
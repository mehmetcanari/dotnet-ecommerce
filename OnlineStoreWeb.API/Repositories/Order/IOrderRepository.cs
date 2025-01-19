public interface IOrderRepository
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderWithIdAsync(int id);
    Task<List<Order>> GetOrdersByUserIdAsync(int userId);
    Task AddOrderAsync(OrderCreateDto createOrderDto);
    Task DeleteOrderAsync(int id);
    Task DeleteOrderWithUserIdAsync(int userId);
    Task UpdateOrderStatusAsync(int id, OrderStatus status);
}
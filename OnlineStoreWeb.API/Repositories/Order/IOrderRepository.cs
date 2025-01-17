public interface IOrderRepository
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderWithIdAsync(int id);
    Task AddOrderAsync(CreateOrderDto createOrderDto);
    Task UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto);
    Task DeleteOrderAsync(int id);
}
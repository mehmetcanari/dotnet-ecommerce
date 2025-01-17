public interface IOrderRepository
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderWithIdAsync(int id);
    Task AddOrderAsync(CreateOrderDto createOrderDto);
    Task DeleteOrderAsync(int id);
}
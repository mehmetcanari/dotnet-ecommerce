public interface IOrderItemRepository
{
    Task<List<OrderItem>> GetAllOrderItemsAsync();
    Task<OrderItem?> GetOrderItemWithIdAsync(int id);
    Task AddOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto);
    Task DeleteOrderItemAsync(int id);
}
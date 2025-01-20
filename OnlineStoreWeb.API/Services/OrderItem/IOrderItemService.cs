public interface IOrderItemService
{
    Task<OrderItem> GetSpecifiedOrderItemsWithUserIdAsync(int userId, int orderItemId);
    Task<List<OrderItem>> GetAllOrderItemsWithUserIdAsync(int userId);
    Task<List<OrderItem>> GetAllOrderItemsAsync();
    Task AddOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto);
    Task DeleteSpecifiedUserOrderItemAsync(int userId, int orderItemId);
    Task DeleteAllUserOrderItemsAsync(int userId);
}
using OnlineStoreWeb.API.DTO.OrderItem;

namespace OnlineStoreWeb.API.Services.OrderItem;

public interface IOrderItemService
{
    Task<Model.OrderItem> GetSpecifiedOrderItemsWithUserIdAsync(int userId, int orderItemId);
    Task<List<Model.OrderItem>> GetAllOrderItemsWithUserIdAsync(int userId);
    Task<List<Model.OrderItem>> GetAllOrderItemsAsync();
    Task AddOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto);
    Task DeleteSpecifiedUserOrderItemAsync(int userId, int orderItemId);
    Task DeleteAllUserOrderItemsAsync(int userId);
}
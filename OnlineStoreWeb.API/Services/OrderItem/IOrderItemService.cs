using OnlineStoreWeb.API.DTO.Request.OrderItem;

namespace OnlineStoreWeb.API.Services.OrderItem;

public interface IOrderItemService
{
    Task CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto);
    Task<IEnumerable<Model.OrderItem>> GetAllOrderItemsAsync();
    Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto);
    Task DeleteOrderItemAsync(int orderItemId);
}
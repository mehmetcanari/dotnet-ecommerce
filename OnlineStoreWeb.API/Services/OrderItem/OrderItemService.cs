using OnlineStoreWeb.API.DTO.OrderItem;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.OrderItem;

public class OrderItemService(
    IOrderItemRepository orderItemRepository,
    IProductRepository productRepository,
    ILogger<OrderItemService> logger)
    : IOrderItemService
{
    public async Task<List<Model.OrderItem>> GetAllOrderItemsAsync()
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            return orderItems;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching all order items");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Model.OrderItem>> GetAllOrderItemsWithUserIdAsync(int userId)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();
            return userOrderItems;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching all order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Model.OrderItem> GetSpecifiedOrderItemsWithUserIdAsync(int userId, int orderItemId)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();

            Model.OrderItem orderItem = userOrderItems.FirstOrDefault(o => o.Id == orderItemId)
                                        ?? throw new Exception("OrderItem not found");
            return orderItem;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching specified order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddOrderItemAsync(CreateOrderItemDto createOrderItemRequest)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.Product> products = await productRepository.Get();

            Model.Product fetchedProduct = products.FirstOrDefault(p => p.Id == createOrderItemRequest.ProductId)
                                           ?? throw new Exception("Product not found");

            if(orderItems.Any(o => o.UserId == createOrderItemRequest.UserId && o.ProductId == createOrderItemRequest.ProductId)) //Duplicate order item check
            {
                throw new Exception("OrderItem already exists");
            }

            Model.OrderItem orderItem = new Model.OrderItem
            {
                Quantity = createOrderItemRequest.Quantity,
                UserId = createOrderItemRequest.UserId,
                ProductId = createOrderItemRequest.ProductId,
                Price = fetchedProduct.Price,
                OrderItemUpdated = DateTime.UtcNow
            };

            await orderItemRepository.Add(orderItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding order item: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.Product> products = await productRepository.Get();

            Model.Product fetchedProduct = products.FirstOrDefault(p => p.Id == updateOrderItemRequest.ProductId)
                                           ?? throw new Exception("Product not found");

            Model.OrderItem orderItem = orderItems.FirstOrDefault(
                                            o => o.Id == updateOrderItemRequest.Id 
                                                 && o.UserId == updateOrderItemRequest.UserId)

                                        ?? throw new Exception("OrderItem not found");

            orderItem.Quantity = updateOrderItemRequest.Quantity;
            orderItem.ProductId = updateOrderItemRequest.ProductId;
            orderItem.Price = fetchedProduct.Price;
            orderItem.OrderItemUpdated = DateTime.UtcNow;

            await orderItemRepository.Update(orderItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating order item: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteSpecifiedUserOrderItemAsync(int userId, int orderItemId)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            Model.OrderItem orderItem = orderItems.FirstOrDefault(o => o.UserId == userId && o.Id == orderItemId)
                                        ?? throw new Exception("OrderItem not found");

            await orderItemRepository.Delete(orderItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting specified order item with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllUserOrderItemsAsync(int userId)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();
            foreach(Model.OrderItem orderItem in userOrderItems)
            {
                await orderItemRepository.Delete(orderItem);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting all order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllOrderItemsAsync()
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            foreach(Model.OrderItem orderItem in orderItems)
            {
                await orderItemRepository.Delete(orderItem);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting all order items");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
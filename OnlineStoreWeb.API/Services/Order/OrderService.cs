using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.OrderItem;

namespace OnlineStoreWeb.API.Services.Order;

public class OrderService(
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository,
    ILogger<OrderService> logger)
    : IOrderService
{
    public async Task AddOrderAsync(OrderCreateDto createOrderDto)
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemRepository.Get();
            List<Model.OrderItem> orderItemsByUserId = orderItems.Where(o => o.UserId == createOrderDto.UserId).ToList();

            if(orderItemsByUserId.Count == 0)
            {
                logger.LogError("No order items found for the user");
                throw new Exception("No order items found for the user");
            }

            Model.Order order = new Model.Order
            {
                UserId = createOrderDto.UserId,
                ShippingAddress = createOrderDto.ShippingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                Status = OrderStatus.Pending,
                OrderItems = orderItemsByUserId
            };
            await orderRepository.Add(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderAsync(int id)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            await orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderWithUserIdAsync(int userId)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.UserId == userId) ?? throw new Exception("Order not found");
            await orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting order with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Model.Order>> GetAllOrdersAsync()
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            return orders;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Model.Order>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            List<Model.Order> ordersByUserId = orders.Where(o => o.UserId == userId).ToList();
            return ordersByUserId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching orders by user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Model.Order> GetOrderWithIdAsync(int id)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            return order;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            order.Status = status;
            await orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
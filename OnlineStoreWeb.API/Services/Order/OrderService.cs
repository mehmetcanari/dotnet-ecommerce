using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.Order;

public class OrderService(
    IOrderRepository orderRepository,
    IAccountRepository accountRepository, 
    IProductRepository productRepository, 
    ILogger<OrderService> logger) : IOrderService
{
    public async Task AddOrderAsync(OrderCreateDto createOrderDto)
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            Model.Product product = products.FirstOrDefault(p => p.Id == createOrderDto.ProductId) ??
                                    throw new Exception("Product not found");

            List<Model.Account> accounts = await accountRepository.Get();
            Model.Account account = accounts.FirstOrDefault(a => a.Id == createOrderDto.UserId) ??
                                    throw new Exception("User not found");

            if (createOrderDto.Quantity > product.StockQuantity)
            {
                throw new Exception("Quantity exceeds stock quantity");
            }
            
            Model.Order order = new Model.Order
            {
                UserId = createOrderDto.UserId, 
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress, 
                PaymentMethod = createOrderDto.PaymentMethod,
                ProductId = createOrderDto.ProductId,
                Price = product.Price,
                Quantity = createOrderDto.Quantity,
                AccountName = account.FullName
            };
            
            await orderRepository.Add(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderAsync(int userId)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.Id == userId) ?? throw new Exception("Order not found");
            await orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
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

    public async Task UpdateOrderStatusAsync(int id, OrderUpdateDto orderUpdateDto)
    {
        try
        {
            List<Model.Order> orders = await orderRepository.Get();
            Model.Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            order.Status = orderUpdateDto.Status;
            await orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
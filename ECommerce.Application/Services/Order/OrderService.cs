using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.DTO.Response.OrderItem;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Application.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemService _orderItemService;
    private readonly IProductService _productService;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public OrderService(
        IOrderRepository orderRepository, 
        IOrderItemService orderItemService,
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository,
        IProductService productService,
        IAccountRepository accountRepository, 
        ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _orderItemRepository = orderItemRepository;
        _orderItemService = orderItemService;
        _productRepository = productRepository;
        _productService = productService;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateRequestDto createRequestOrderDto, string email)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("User not found");
            var userOrderItems = orderItems
                .Where(oi => !oi.IsOrdered)
                .Where(oi => oi.AccountId == tokenAccount.AccountId)
                .ToList();

            List<Domain.Model.OrderItem> newOrderItems = userOrderItems
                .Select(orderItem => new Domain.Model.OrderItem
                {
                    AccountId = orderItem.AccountId,
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    ProductName = orderItem.ProductName,
                    IsOrdered = true
                }).ToList();

            if (newOrderItems.Count == 0)
            {
                throw new Exception("No items in cart");
            }

            var order = new Domain.Model.Order
            {
                AccountId = tokenAccount.AccountId,
                ShippingAddress = createRequestOrderDto.ShippingAddress,
                BillingAddress = createRequestOrderDto.BillingAddress,
                PaymentMethod = createRequestOrderDto.PaymentMethod,
                OrderItems = newOrderItems
            };

            await _orderRepository.Create(order);
            await _orderItemService.DeleteAllOrderItemsAsync(email);
            await _productService.UpdateProductStockAsync(newOrderItems);
            await _productService.ProductCacheInvalidateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw;
        }
    }

    public async Task CancelOrderAsync(string email, OrderCancelRequestDto orderCancelRequestDto)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("Account not found");
            var order = orders.FirstOrDefault(o => o.AccountId == tokenAccount.AccountId) ?? throw new Exception("Order not found");

            await UpdateOrderStatusByAccountIdAsync(tokenAccount.AccountId, 
            new OrderUpdateRequestDto 
            { 
                Status = orderCancelRequestDto.Status 
            });

            _logger.LogInformation("Order cancelled successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteOrderByIdAsync(int id)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var orderToDelete = orders.FirstOrDefault(o => o.OrderId == id) ?? throw new Exception("Order not found");
            await _orderRepository.Delete(orderToDelete);

            _logger.LogInformation("Order deleted successfully: {Order}", orderToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.Read();
            if (orders.Count == 0)
            {
                throw new Exception("No orders found");
            }

            return orders.Select(o => new OrderResponseDto
            {
                AccountId = o.AccountId,
                OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = o.OrderDate,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                PaymentMethod = o.PaymentMethod,
                Status = o.Status
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetUserOrdersAsync(string email)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                               throw new Exception("Account not found");
            
            var userOrders = orders.Where(o => o.AccountId == tokenAccount.AccountId).ToList();
            var orderedItems = userOrders.Where(o => o.OrderItems.Any(oi => oi.IsOrdered == true)).ToList();
            
            if (orderedItems.Count == 0)
                throw new Exception("No orders found for this user");

            return orderedItems.Select(order => new OrderResponseDto
            {
                AccountId = order.AccountId,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user orders: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(int id)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.OrderId == id) ??
                        throw new Exception("Order not found");

            OrderResponseDto orderResponseDto = new()
            {
                AccountId = order.AccountId,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status
            };

            return orderResponseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.AccountId == accountId) ?? throw new Exception("Order not found");
            order.Status = orderUpdateRequestDto.Status;
            await _orderRepository.Update(order);

            _logger.LogInformation("Order status updated successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw;
        }
    }
}
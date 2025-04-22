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
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository, 
        IOrderItemService orderItemService,
        IOrderItemRepository orderItemRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        IAccountRepository accountRepository, 
        ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _orderItemRepository = orderItemRepository;
        _unitOfWork = unitOfWork;
        _orderItemService = orderItemService;
        _productService = productService;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateRequestDto createRequestOrderDto, string email)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("User not found");
            var userOrderItems = orderItems
                .Where(oi => oi.AccountId == tokenAccount.AccountId)
                .Where(oi => oi.IsOrdered == false)
                .ToList();

            if (userOrderItems.Count == 0)
            {
                _logger.LogWarning("No order items found for this user: {Email}", email);
                throw new Exception("No order items found for this user");
            }

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
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order added successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw;
        }
    }

    public async Task CancelOrderAsync(string email)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                throw new Exception("Account not found");

            var pendingOrders = orders
                .Where(o => o.AccountId == tokenAccount.AccountId && o.Status == OrderStatus.Pending)
                .ToList();

            if (!pendingOrders.Any())
            {
                throw new Exception("No pending orders found");
            }

            foreach (var order in pendingOrders)
            {
                order.Status = OrderStatus.Cancelled;
                _orderRepository.Update(order);
            }

            await _unitOfWork.Commit();

            _logger.LogInformation("Orders cancelled successfully. Count: {Count}", pendingOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while cancelling orders: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteOrderByIdAsync(int id)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var orderToDelete = orders.FirstOrDefault(o => o.OrderId == id) ?? throw new Exception("Order not found");
            _orderRepository.Delete(orderToDelete);

            await _unitOfWork.Commit();
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
            _orderRepository.Update(order);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order status updated successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw;
        }
    }
}
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Application.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly IProductService _productService;
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository, 
        IBasketItemService basketItemService,
        IBasketItemRepository basketItemRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        IAccountRepository accountRepository, 
        ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _basketItemRepository = basketItemRepository;
        _unitOfWork = unitOfWork;
        _basketItemService = basketItemService;
        _productService = productService;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateRequestDto createRequestOrderDto, string email)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var basketItems = await _basketItemRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("User not found");
            var userBasketItems = basketItems
                .Where(oi => oi.AccountId == tokenAccount.Id)
                .Where(oi => oi.IsOrdered == false)
                .ToList();

            if (userBasketItems.Count == 0)
            {
                _logger.LogWarning("No basket items found for this user: {Email}", email);
                throw new Exception("No basket items found for this user");
            }

            List<Domain.Model.BasketItem> newBasketItems = userBasketItems
                .Select(basketItem => new Domain.Model.BasketItem
                {
                    AccountId = basketItem.AccountId,
                    ProductId = basketItem.ProductId,
                    Quantity = basketItem.Quantity,
                    UnitPrice = basketItem.UnitPrice,
                    ProductName = basketItem.ProductName,
                    IsOrdered = true
                }).ToList();

            Domain.Model.Order order = new Domain.Model.Order
            {
                AccountId = tokenAccount.Id,
                ShippingAddress = createRequestOrderDto.ShippingAddress,
                BillingAddress = createRequestOrderDto.BillingAddress,
                PaymentMethod = createRequestOrderDto.PaymentMethod,
                BasketItems = newBasketItems
            };

            await _orderRepository.Create(order);
            await _basketItemService.DeleteAllBasketItemsAsync(email);
            await _productService.UpdateProductStockAsync(newBasketItems);
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
                .Where(o => o.AccountId == tokenAccount.Id && o.Status == OrderStatus.Pending)
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
                BasketItems = o.BasketItems.Select(oi => new BasketItemResponseDto
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
            
            var userOrders = orders.Where(o => o.AccountId == tokenAccount.Id).ToList();
            var orderedItems = userOrders.Where(o => o.BasketItems.Any(oi => oi.IsOrdered == true)).ToList();
            
            if (orderedItems.Count == 0)
                throw new Exception("No orders found for this user");

            return orderedItems.Select(order => new OrderResponseDto
            {
                AccountId = order.AccountId,
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
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
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
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
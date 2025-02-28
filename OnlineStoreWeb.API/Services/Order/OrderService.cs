using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.DTO.Response.Order;
using OnlineStoreWeb.API.DTO.Response.OrderItem;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;
using OnlineStoreWeb.API.Services.OrderItem;

namespace OnlineStoreWeb.API.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemService _orderItemService;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, IOrderItemService orderItemService,IOrderItemRepository orderItemRepository,
        IAccountRepository accountRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _orderItemRepository = orderItemRepository;
        _orderItemService = orderItemService;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateDto createOrderDto)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();

            var userAccount = accounts.FirstOrDefault(a => a.AccountId == createOrderDto.AccountId) ??
                              throw new Exception("User not found");
            var userOrderItems = orderItems.Where(oi => oi.AccountId == userAccount.AccountId).ToList();

            List<Model.OrderItem> newOrderItems = userOrderItems
                .Select(cartItem => new Model.OrderItem
                {
                    AccountId = cartItem.AccountId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    ProductName = cartItem.ProductName
                }).ToList();

            if (newOrderItems.Count == 0)
            {
                throw new Exception("No items in cart");
            }

            Model.Order order = new Model.Order
            {
                AccountId = userAccount.AccountId,
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                OrderItems = newOrderItems
            };
            
            await _orderItemService.DeleteAllOrderItemsByAccountIdAsync(userAccount.AccountId);
            await _orderRepository.Create(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderByAccountIdAsync(int accountId)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.AccountId == accountId) ??
                        throw new Exception("Order not found");
            await _orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.Read();
            
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
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<OrderResponseDto> GetOrderWithIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(id);
            
            if (order == null)
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
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderStatusAsync(int id, OrderUpdateDto orderUpdateDto)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.OrderId == id) ?? throw new Exception("Order not found");
            order.Status = orderUpdateDto.Status;
            await _orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}
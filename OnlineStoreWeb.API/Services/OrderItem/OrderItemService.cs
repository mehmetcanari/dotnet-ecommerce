using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.DTO.Response.OrderItem;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.OrderItem;

public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderItemService> _logger;

    public OrderItemService(IOrderItemRepository orderItemRepository, IProductRepository productRepository,
        ILogger<OrderItemService> logger, IAccountRepository accountRepository)
    {
        _orderItemRepository = orderItemRepository;
        _logger = logger;
        _productRepository = productRepository;
        _accountRepository = accountRepository;
    }

    public async Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync()
    {
        try
        {
            IEnumerable<Model.OrderItem> orderItems = await _orderItemRepository.Read();
            var items = orderItems.ToList();
            if (items.Count == 0)
            {
                throw new Exception("No order items found.");
            }

            return items.Select(orderItem => new OrderItemResponseDto
            {
                AccountId = orderItem.AccountId,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName
            }).ToList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while fetching all order items");
            throw new Exception(exception.Message);
        }
    }

    public async Task CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto)
    {
        try
        {
            var products = await _productRepository.Read();
            var accounts = await _accountRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == createOrderItemDto.ProductId);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (accounts.FirstOrDefault(a => a.AccountId == createOrderItemDto.AccountId) == null)
                throw new Exception("Account not found");

            if (createOrderItemDto.Quantity > product.StockQuantity)
            {
                throw new Exception("Not enough stock");
            }

            var orderItem = new Model.OrderItem
            {
                AccountId = createOrderItemDto.AccountId,
                Quantity = createOrderItemDto.Quantity,
                ProductId = product.ProductId,
                UnitPrice = product.Price,
                ProductName = product.Name
            };

            await _orderItemRepository.Create(orderItem);
            _logger.LogInformation("Order item created successfully");

            product.StockQuantity -= createOrderItemDto.Quantity;
            await _productRepository.Update(product);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating order item");
            throw new Exception(exception.Message);
        }
    }

    public async Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto)
    {
        try
        {
            var products = await _productRepository.Read();
            var orderItems = await _orderItemRepository.Read();

            var orderItem = orderItems.FirstOrDefault(p =>
                                p.OrderItemId == updateOrderItemDto.OrderItemId &&
                                p.AccountId == updateOrderItemDto.AccountId) ??
                            throw new Exception("Order item not found");

            var product = products.FirstOrDefault(p => p.ProductId == updateOrderItemDto.ProductId) ??
                          throw new Exception("Product not found");

            if (product.StockQuantity < updateOrderItemDto.Quantity)
            {
                throw new Exception("Not enough stock");
            }

            orderItem.Quantity = updateOrderItemDto.Quantity;
            orderItem.ProductId = updateOrderItemDto.ProductId;
            orderItem.ProductName = product.Name;

            await _orderItemRepository.Update(orderItem);
            _logger.LogInformation("Order item updated successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating order item");
            throw new Exception(exception.Message);
        }
    }

    public async Task DeleteAllOrderItemsByAccountIdAsync(int id)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var items = orderItems.Where(o => o.AccountId == id).ToList();

            foreach (var item in items)
            {
                await _orderItemRepository.Delete(item);
            }

            _logger.LogInformation("Order item deleted successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting order item");
            throw new Exception(exception.Message);
        }
    }
}
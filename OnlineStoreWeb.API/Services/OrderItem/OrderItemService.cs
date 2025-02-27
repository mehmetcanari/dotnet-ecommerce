using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.DTO.Response.OrderItem;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.OrderItem;

public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderItemService> _logger;

    public OrderItemService(IOrderItemRepository orderItemRepository, IProductRepository productRepository,
        ILogger<OrderItemService> logger)
    {
        _orderItemRepository = orderItemRepository;
        _logger = logger;
        _productRepository = productRepository;
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
                Price = orderItem.Price,
                ProductId = orderItem.ProductId,
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
            Model.Product? product = products.FirstOrDefault(p => p.ProductId == createOrderItemDto.ProductId);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (createOrderItemDto.Quantity > product.StockQuantity)
            {
                throw new Exception("Not enough stock");
            }

            var orderItem = new Model.OrderItem
            {
                AccountId = createOrderItemDto.AccountId,
                Quantity = createOrderItemDto.Quantity,
                ProductId = createOrderItemDto.ProductId,
                Price = product.Price,
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
            IEnumerable<Model.OrderItem> orderItems = await _orderItemRepository.Read();
            Model.OrderItem orderItem =
                orderItems.FirstOrDefault(p => p.OrderItemId == updateOrderItemDto.OrderItemId) ??
                throw new Exception("Order item not found");

            IEnumerable<Model.Product?> products = await _productRepository.Read();
            Model.Product? product = products.FirstOrDefault(p => p.ProductId == updateOrderItemDto.ProductId) ??
                                     throw new Exception("Product not found");

            if (product.StockQuantity < updateOrderItemDto.Quantity)
            {
                throw new Exception("Not enough stock");
            }

            orderItem.Quantity = updateOrderItemDto.Quantity;
            orderItem.ProductId = updateOrderItemDto.ProductId;

            await _orderItemRepository.Update(orderItem);
            _logger.LogInformation("Order item updated successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating order item");
            throw new Exception(exception.Message);
        }
    }

    public async Task DeleteOrderItemAsync(int id)
    {
        try
        {
            IEnumerable<Model.OrderItem> orderItems = await _orderItemRepository.Read();
            Model.OrderItem orderItem = orderItems.FirstOrDefault(p => p.OrderItemId == id) ??
                                        throw new Exception("Order item not found");

            await _orderItemRepository.Delete(orderItem);
            _logger.LogInformation("Order item deleted successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting order item");
            throw new Exception(exception.Message);
        }
    }
}
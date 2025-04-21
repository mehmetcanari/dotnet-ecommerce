using ECommerce.Application.DTO.Request.OrderItem;
using ECommerce.Application.DTO.Response.OrderItem;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Application.Services.OrderItem;

public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;
    private const string GetAllOrderItemsCacheKey = "GetAllOrderItems";

    public OrderItemService(
        IOrderItemRepository orderItemRepository, 
        IProductRepository productRepository,
        IAccountRepository accountRepository,
        ILoggingService logger,
        ICacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _accountRepository = accountRepository;
        _logger = logger;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderItemResponseDto>> GetAllOrderItemsAsync(string email)
    {
        try
        {
            var cacheKey = GetAllOrderItemsCacheKey;
            TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
            var cachedItems = await _cacheService.GetAsync<List<OrderItemResponseDto>>(cacheKey);
            if (cachedItems != null)
            {
                _logger.LogInformation("Order items fetched from cache");
                return cachedItems;
            }

            var accounts = await _accountRepository.Read();
            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                        throw new Exception("Account not found");
            var orderItems = await _orderItemRepository.Read();
            var nonOrderedCartItems = orderItems.Where(o => o.AccountId == tokenAccount.AccountId && o.IsOrdered == false).ToList();
            if (nonOrderedCartItems.Count == 0)
            {
                throw new Exception("No order items found.");
            }

            List<OrderItemResponseDto> clientResponseCartItems = nonOrderedCartItems
            .Select(orderItem => new OrderItemResponseDto
            {
                AccountId = orderItem.AccountId,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName
            }).ToList();

            await _cacheService.SetAsync(cacheKey, clientResponseCartItems, cacheDuration);

            return clientResponseCartItems;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while fetching all order items");
            throw;
        }
    }

    public async Task CreateOrderItemAsync(CreateOrderItemRequestDto createOrderItemRequestDto, string email)
    {
        try
        {
            var products = await _productRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ?? throw new Exception("Account not found");
            var product = products.FirstOrDefault(p => p.ProductId == createOrderItemRequestDto.ProductId) ?? throw new Exception("Product not found");

            if (createOrderItemRequestDto.Quantity > product.StockQuantity)
                throw new Exception("Not enough stock");

            var orderItem = new Domain.Model.OrderItem
            {
                AccountId = tokenAccount.AccountId,
                Quantity = createOrderItemRequestDto.Quantity,
                ProductId = product.ProductId,
                UnitPrice = product.Price,
                ProductName = product.Name,
                IsOrdered = false
            };

            await _orderItemRepository.Create(orderItem);
            await ClearOrderItemsCacheAsync();
            await _unitOfWork.Commit();

            _logger.LogInformation("Order item created successfully: {OrderItem}", orderItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating order item");
            throw;
        }
    }

    public async Task UpdateOrderItemAsync(UpdateOrderItemRequestDto updateOrderItemRequestDto, string email)
    {
        try
        {
            var products = await _productRepository.Read();
            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                        throw new Exception("Account not found");

            var orderItem = orderItems.FirstOrDefault(p =>
                                p.OrderItemId == updateOrderItemRequestDto.OrderItemId &&
                                p.AccountId == tokenAccount.AccountId) ??
                            throw new Exception("Order item not found");

            var updatedProduct = products.FirstOrDefault(p => p.ProductId == updateOrderItemRequestDto.ProductId) ??
                          throw new Exception("Product not found");

            if (updatedProduct.StockQuantity < updateOrderItemRequestDto.Quantity)
            {
                throw new Exception("Not enough stock");
            }

            orderItem.Quantity = updateOrderItemRequestDto.Quantity;
            orderItem.ProductId = updateOrderItemRequestDto.ProductId;
            orderItem.ProductName = updatedProduct.Name;
            orderItem.UnitPrice = updatedProduct.Price;
            orderItem.IsOrdered = false;

            _orderItemRepository.Update(orderItem);
            await ClearOrderItemsCacheAsync();
            await _unitOfWork.Commit();

            _logger.LogInformation("Order item updated successfully: {OrderItem}", orderItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating order item");
            throw;
        }
    }

    public async Task DeleteAllOrderItemsAsync(string email)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();
            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                        throw new Exception("Account not found");
            var items = orderItems.Where(o => o.AccountId == tokenAccount.AccountId && o.IsOrdered == false).ToList();

            if (items.Count == 0)
                throw new Exception("No order items found to delete");

            foreach (var item in items)
            {
                _orderItemRepository.Delete(item);
            }

            await ClearOrderItemsCacheAsync();
            await _unitOfWork.Commit();

            _logger.LogInformation("All order items deleted successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting order items");
            throw;
        }
    }

    public async Task ClearOrderItemsIncludeProductAsync(Domain.Model.Product updatedProduct)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var accountCartItems = orderItems.Where(o => o.IsOrdered == false).ToList();

            var orderItem = accountCartItems.FirstOrDefault(i => i.ProductId == updatedProduct.ProductId);
            if (orderItem != null)
            {
                _orderItemRepository.Delete(orderItem);
            }

            await ClearOrderItemsCacheAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Order items cleared successfully for product: {ProductId}", updatedProduct.ProductId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while clearing order items include product");
            throw;
        }
    }

    public async Task ClearOrderItemsCacheAsync()
    {
        await _cacheService.RemoveAsync(GetAllOrderItemsCacheKey);
    }
}
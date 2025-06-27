using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Events;
using MediatR;

namespace ECommerce.Application.Services.Search.Product;

public class ProductElasticsearchEventHandler : 
    INotificationHandler<ProductCreatedEvent>,
    INotificationHandler<ProductUpdatedEvent>,
    INotificationHandler<ProductStockUpdatedEvent>,
    INotificationHandler<ProductDeletedEvent>
{
    private readonly IProductSearchService _productSearchService;
    private readonly ILoggingService _logger;

    public ProductElasticsearchEventHandler(
        IProductSearchService productSearchService,
        ILoggingService logger)
    {
        _productSearchService = productSearchService;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Domain.Model.Product
            {
                ProductId = notification.ProductId,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId,
                ProductCreated = notification.ProductCreated,
                ProductUpdated = notification.ProductUpdated
            };

            var result = await _productSearchService.IndexProductAsync(product);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully indexed new product {ProductId} in Elasticsearch", notification.ProductId);
            }
            else
            {
                _logger.LogError(new Exception(result.Error), "Failed to index new product {ProductId} in Elasticsearch: {Error}", 
                    notification.ProductId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductCreatedEvent for product {ProductId}: {Message}", 
                notification.ProductId, ex.Message);
        }
    }

    public async Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling ProductUpdatedEvent for product {ProductId}", notification.ProductId);
            
            var product = new Domain.Model.Product
            {
                ProductId = notification.ProductId,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId,
                ProductCreated = notification.ProductCreated,
                ProductUpdated = notification.ProductUpdated
            };

            var result = await _productSearchService.UpdateProductAsync(product);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated product {ProductId} in Elasticsearch", notification.ProductId);
            }
            else
            {
                _logger.LogError(new Exception(result.Error), "Failed to update product {ProductId} in Elasticsearch: {Error}", 
                    notification.ProductId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductUpdatedEvent for product {ProductId}: {Message}", 
                notification.ProductId, ex.Message);
        }
    }

    public async Task Handle(ProductStockUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling ProductStockUpdatedEvent for product {ProductId}", notification.ProductId);
            
            var product = new Domain.Model.Product
            {
                ProductId = notification.ProductId,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId,
                ProductCreated = notification.ProductCreated,
                ProductUpdated = notification.ProductUpdated
            };

            var result = await _productSearchService.UpdateProductAsync(product);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated product stock for {ProductId} in Elasticsearch", notification.ProductId);
            }
            else
            {
                _logger.LogError(new Exception(result.Error), "Failed to update product stock for {ProductId} in Elasticsearch: {Error}", 
                    notification.ProductId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductStockUpdatedEvent for product {ProductId}: {Message}", 
                notification.ProductId, ex.Message);
        }
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling ProductDeletedEvent for product {ProductId}", notification.ProductId);
            
            var result = await _productSearchService.DeleteProductAsync(notification.ProductId.ToString());
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted product {ProductId} from Elasticsearch", notification.ProductId);
            }
            else
            {
                _logger.LogError(new Exception(result.Error), "Failed to delete product {ProductId} from Elasticsearch: {Error}", 
                    notification.ProductId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductDeletedEvent for product {ProductId}: {Message}", 
                notification.ProductId, ex.Message);
        }
    }
} 
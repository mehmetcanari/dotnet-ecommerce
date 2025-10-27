using ECommerce.Application.Abstract;
using ECommerce.Application.Events;
using MediatR;

namespace ECommerce.Application.Services.Search.Product;

public class ProductElasticsearchEventHandler(IElasticSearchService elasticSearchService, ILogService logger) : INotificationHandler<ProductCreatedEvent>, INotificationHandler<ProductUpdatedEvent>, INotificationHandler<ProductStockUpdatedEvent>, INotificationHandler<ProductDeletedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Domain.Model.Product
            {
                Id = notification.ProductId,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId
            };

            var result = await elasticSearchService.IndexProductAsync(product);
            if (result.IsFailure)
                logger.LogWarning("Failed to index new product {ProductId} in Elasticsearch: {Error}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProductCreatedEvent for product {ProductId}: {Message}", notification.ProductId, ex.Message);
        }
    }

    public async Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling ProductUpdatedEvent for product {ProductId}", notification.Id);
            
            var product = new Domain.Model.Product
            {
                Id = notification.Id,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId
            };

            var result = await elasticSearchService.UpdateProductAsync(product);
            if (result.IsFailure)
                logger.LogWarning("Failed to update product {ProductId} in Elasticsearch: {Error}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProductUpdatedEvent for product {ProductId}: {Message}", notification.Id, ex.Message);
        }
    }

    public async Task Handle(ProductStockUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {            
            var product = new Domain.Model.Product
            {
                Id = notification.ProductId,
                Name = notification.Name,
                Description = notification.Description,
                Price = notification.Price,
                DiscountRate = notification.DiscountRate,
                ImageUrl = notification.ImageUrl,
                StockQuantity = notification.StockQuantity,
                CategoryId = notification.CategoryId
            };

            var result = await elasticSearchService.UpdateProductAsync(product);
            if (result.IsFailure)
                logger.LogWarning("Failed to update product stock {ProductId} in Elasticsearch: {Error}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProductStockUpdatedEvent for product {ProductId}: {Message}", notification.ProductId, ex.Message);
        }
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling ProductDeletedEvent for product {ProductId}", notification.Id);
            
            var result = await elasticSearchService.DeleteProductAsync(notification.Id.ToString());
            if (result.IsFailure)
                logger.LogWarning("Failed to delete product {ProductId} in Elasticsearch: {Error}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProductDeletedEvent for product {ProductId}: {Message}",  notification.Id, ex.Message);
        }
    }
} 
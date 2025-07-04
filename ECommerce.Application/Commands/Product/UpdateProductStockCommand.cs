using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductStockCommand : IRequest<Result>
{
    public required List<BasketItem> BasketItems { get; set; }
}

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediator _mediator;
    private readonly ILoggingService _logger;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        IMediator mediator,
        ILoggingService logger)
    {
        _productRepository = productRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProducts = new List<Domain.Model.Product>();
            
            foreach (var item in request.BasketItems)
            {
                var validationResult = await ValidateAndUpdateStock(item);
                if (!validationResult.result.IsSuccess)
                {
                    return validationResult.result;
                }
                updatedProducts.Add(validationResult.product);
            }

            foreach (var product in updatedProducts)
            {
                var domainEvent = new ProductStockUpdatedEvent
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    DiscountRate = product.DiscountRate,
                    ImageUrl = product.ImageUrl,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    ProductCreated = product.ProductCreated,
                    ProductUpdated = product.ProductUpdated
                };

                await _mediator.Publish(domainEvent, cancellationToken);
            }

            _logger.LogInformation("Product stock updated successfully for {Count} items. Events published for Elasticsearch updating.", request.BasketItems.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product stock: {Message}", ex.Message);
            return Result.Failure("An error occurred while updating product stock");
        }
    }

    private async Task<(Result result, Domain.Model.Product product)> ValidateAndUpdateStock(BasketItem item)
    {
        var product = await _productRepository.GetProductById(item.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product stock update failed. Product with ID {ProductId} not found", item.ProductId);
            return (Result.Failure($"Product with ID {item.ProductId} not found"), null!);
        }

        if (product.StockQuantity < item.Quantity)
        {
            _logger.LogWarning(
                "Product stock update failed. Insufficient stock for product {ProductName}. Available: {Available}, Requested: {Requested}",
                product.Name, product.StockQuantity, item.Quantity);
            return (Result.Failure($"Insufficient stock for product {product.Name}"), null!);
        }

        UpdateProductStock(product, item.Quantity);
        return (Result.Success(), product);
    }

    private void UpdateProductStock(Domain.Model.Product product, int quantity)
    {
        product.StockQuantity -= quantity;
        product.ProductUpdated = DateTime.UtcNow;
        _productRepository.UpdateStock(product.ProductId, product.StockQuantity);

        _logger.LogInformation(
            "Product stock updated. ProductId: {ProductId}, Name: {Name}, NewStock: {NewStock}",
            product.ProductId, product.Name, product.StockQuantity);
    }
}
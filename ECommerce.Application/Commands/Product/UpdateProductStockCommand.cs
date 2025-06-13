using ECommerce.Application.Abstract.Service;
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
    private readonly ILoggingService _logger;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        ILoggingService logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in request.BasketItems)
            {
                var validationResult = await ValidateAndUpdateStock(item);
                if (!validationResult.IsSuccess)
                {
                    return validationResult;
                }
            }

            _logger.LogInformation("Product stock updated successfully for {Count} items", request.BasketItems.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product stock: {Message}", ex.Message);
            return Result.Failure("An error occurred while updating product stock");
        }
    }

    private async Task<Result> ValidateAndUpdateStock(BasketItem item)
    {
        var product = await _productRepository.GetProductById(item.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product stock update failed. Product with ID {ProductId} not found", item.ProductId);
            return Result.Failure($"Product with ID {item.ProductId} not found");
        }

        if (product.StockQuantity < item.Quantity)
        {
            _logger.LogWarning(
                "Product stock update failed. Insufficient stock for product {ProductName}. Available: {Available}, Requested: {Requested}",
                product.Name, product.StockQuantity, item.Quantity);
            return Result.Failure($"Insufficient stock for product {product.Name}");
        }

        UpdateProductStock(product, item.Quantity);
        return Result.Success();
    }

    private void UpdateProductStock(Domain.Model.Product product, int quantity)
    {
        product.StockQuantity -= quantity;
        _productRepository.Update(product);

        _logger.LogInformation(
            "Product stock updated. ProductId: {ProductId}, Name: {Name}, NewStock: {NewStock}",
            product.ProductId, product.Name, product.StockQuantity);
    }
}
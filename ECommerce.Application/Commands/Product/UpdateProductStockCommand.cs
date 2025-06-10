using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductStockCommand : IRequest<Result>
{
    public required List<ECommerce.Domain.Model.BasketItem> BasketItems { get; set; }
}

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ILoggingService _logger;

    public UpdateProductStockCommandHandler(IProductRepository productRepository, ILoggingService logger)
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
                var product = await _productRepository.GetProductById(item.ProductId);
                if (product == null)
                {
                    return Result.Failure($"Product with ID {item.ProductId} not found");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return Result.Failure($"Insufficient stock for product {product.Name}");
                }

                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product stock");
            return Result.Failure("An error occurred while updating product stock");
        }
    }
}
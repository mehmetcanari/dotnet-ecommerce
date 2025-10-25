using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductStockCommand : IRequest<Result>
{
    public required List<BasketItem> Model { get; init; }
}

public class UpdateProductStockCommandHandler(IProductRepository productRepository, IMediator mediator, ILoggingService logger) : IRequestHandler<UpdateProductStockCommand, Result>
{
    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProducts = new List<Domain.Model.Product>();

            foreach (var item in request.Model)
            {
                var validation = await ValidateAndUpdateStock(item, cancellationToken);
                if (!validation.result.IsSuccess)
                    return validation.result;

                updatedProducts.Add(validation.product);
            }

            await PublishProductStockUpdatedEvents(updatedProducts, cancellationToken);

            logger.LogInformation(ErrorMessages.UpdateProductStockSuccess, request.Model.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingProductStock, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProductStock);
        }
    }

    private async Task<(Result result, Domain.Model.Product product)> ValidateAndUpdateStock(BasketItem item, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetById(item.ProductId, cancellationToken);
        if (product == null)
            return (Result.Failure(ErrorMessages.ProductNotFound), null!);

        if (product.StockQuantity < item.Quantity)
            return (Result.Failure(ErrorMessages.StockNotAvailable), null!);

        await UpdateProductStock(product, item.Quantity, cancellationToken);
        return (Result.Success(), product);
    }

    private async Task UpdateProductStock(Domain.Model.Product product, int quantity, CancellationToken cancellationToken)
    {
        product.StockQuantity -= quantity;
        product.UpdatedOn = DateTime.UtcNow;
        await productRepository.Update(product, cancellationToken);

        logger.LogInformation(ErrorMessages.UpdateProductStockSuccess, product.Id, product.Name, product.StockQuantity);
    }

    private async Task PublishProductStockUpdatedEvents(IEnumerable<Domain.Model.Product> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
        {
            var stockUpdateEvent = new ProductStockUpdatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl ?? string.Empty,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ProductCreated = product.CreatedOn,
                ProductUpdated = product.UpdatedOn
            };

            await mediator.Publish(stockUpdateEvent, cancellationToken);
        }
    }
}
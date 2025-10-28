using ECommerce.Application.Abstract;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductStockCommand(List<BasketItem> request) : IRequest<Result>
{
    public readonly List<BasketItem> Model = request;
}

public class UpdateProductStockCommandHandler(IProductRepository productRepository, IMediator mediator, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductStockCommand, Result>
{
    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProducts = new List<Domain.Model.Product>();

            foreach (var item in request.Model)
            {
                var product = await productRepository.GetById(item.ProductId, cancellationToken);
                if (product is null)
                    return Result.Failure(ErrorMessages.ProductNotFound);

                if (item.Quantity > product.StockQuantity)
                    return Result.Failure(ErrorMessages.StockNotAvailable);

                product.StockQuantity -= item.Quantity;
                if(product.StockQuantity < 0)
                    product.StockQuantity = 0;

                product.UpdatedOn = DateTime.UtcNow;
                await productRepository.Update(product, cancellationToken);
                updatedProducts.Add(product);
            }

            await unitOfWork.Commit();
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
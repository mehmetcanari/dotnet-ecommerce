using ECommerce.Application.Abstract;
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

public class UpdateProductStockCommandHandler(IProductRepository productRepository, ILogService logger, IUnitOfWork unitOfWork, IElasticSearchService elasticSearchService) : IRequestHandler<UpdateProductStockCommand, Result>
{
    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
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
                await elasticSearchService.UpdateAsync(product.Id.ToString(), product, "products", cancellationToken);
                await productRepository.Update(product, cancellationToken);
            }

            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingProductStock, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProductStock);
        }
    }
}
using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class DeleteProductCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class DeleteProductCommandHandler(IProductRepository productRepository, ILogService logger, IUnitOfWork unitOfWork, IElasticSearchService elasticSearchService) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productRepository.GetById(request.Id, cancellationToken);
            if (product is null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            await elasticSearchService.DeleteAsync<Domain.Model.Product>(product.Id.ToString(), "products", cancellationToken);
            await productRepository.Delete(product, cancellationToken);
            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorDeletingCategory, request.Id);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}

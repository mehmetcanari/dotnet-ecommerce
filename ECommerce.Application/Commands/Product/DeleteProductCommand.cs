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

public class DeleteProductCommandHandler(IProductRepository productRepository, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var productResult = await ValidateAndGetProduct(request);
            if (productResult is { IsFailure: true, Message: not null })
                return Result.Failure(productResult.Message);

            if (productResult.Data is null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            await productRepository.Delete(productResult.Data, cancellationToken);
            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorDeletingCategory, request.Id);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Product>> ValidateAndGetProduct(DeleteProductCommand request)
    {
        var product = await productRepository.GetById(request.Id);
        if (product == null)
            return Result<Domain.Model.Product>.Failure(ErrorMessages.ProductNotFound);

        return Result<Domain.Model.Product>.Success(product);
    }
}

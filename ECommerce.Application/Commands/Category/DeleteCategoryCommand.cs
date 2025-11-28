using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class DeleteCategoryCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoryRepository.GetById(request.Id, cancellationToken);
            if (category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            categoryRepository.Delete(category);
            await cache.RemoveAsync(CacheKeys.Category, cancellationToken);
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
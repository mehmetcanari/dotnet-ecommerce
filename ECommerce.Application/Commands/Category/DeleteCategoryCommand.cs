using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class DeleteCategoryCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryResult = await ValidateAndGetCategory(request);
            if (categoryResult is { IsFailure: true, Message: not null })
                return Result.Failure(categoryResult.Message);

            if (categoryResult.Data is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            categoryRepository.Delete(categoryResult.Data);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorDeletingCategory, request.Id);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(DeleteCategoryCommand request)
    {
        var category = await categoryRepository.GetById(request.Id);
        if (category == null)
        {
            logger.LogWarning(ErrorMessages.CategoryNotFound, request.Id);
            return Result<Domain.Model.Category>.Failure(ErrorMessages.CategoryNotFound);
        }

        return Result<Domain.Model.Category>.Success(category);
    }
}
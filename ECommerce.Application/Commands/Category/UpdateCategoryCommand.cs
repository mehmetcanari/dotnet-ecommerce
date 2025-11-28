using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class UpdateCategoryCommand(UpdateCategoryRequestDto request) : IRequest<Result>
{
    public readonly UpdateCategoryRequestDto Model = request;
}

public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryExists = await categoryRepository.CheckNameExists(request.Model.Name, cancellationToken);
            if (categoryExists)
                return Result.Failure(ErrorMessages.CategoryExists);

            var category = await categoryRepository.GetById(request.Model.Id, cancellationToken);
            if (category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            category.Name = request.Model.Name;
            category.Description = request.Model.Description;

            await cache.RemoveAsync(CacheKeys.Category, cancellationToken);
            categoryRepository.Update(category);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingCategory, request.Model.Id);
            return Result.Failure(ErrorMessages.ErrorUpdatingCategory);
        }
    }
}
using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class UpdateCategoryCommand(UpdateCategoryRequestDto request) : IRequest<Result>
{
    public readonly UpdateCategoryRequestDto Model = request;
}

public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryExists = await categoryRepository.CheckNameExists(request.Model.Name, cancellationToken);
            if (categoryExists)
                return Result.Failure(ErrorMessages.CategoryExists);

            var category = await categoryRepository.GetById(request.Model.Id, cancellationToken);
            if(category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            category.Name = request.Model.Name;
            category.Description = request.Model.Description;

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
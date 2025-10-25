using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class CreateCategoryCommand(CreateCategoryRequestDto request) : IRequest<Result>
{
    public readonly CreateCategoryRequestDto Model = request;
}

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger) : IRequestHandler<CreateCategoryCommand, Result>
{
    public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateCategoryName(request);
            if (validationResult.IsFailure)
                return validationResult;

            var category = CreateCategoryEntity(request);
            await SaveCategory(category);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingCategory);
            return Result.Failure(ErrorMessages.ErrorCreatingCategory);
        }
    }

    private async Task<Result> ValidateCategoryName(CreateCategoryCommand request)
    {

        var categoryExists = await categoryRepository.CheckNameExists(request.Model.Name);
        if (categoryExists)
            return Result.Failure(ErrorMessages.CategoryExists);

        return Result.Success();
    }

    private static Domain.Model.Category CreateCategoryEntity(CreateCategoryCommand request) => new()
    {
        Name = request.Model.Name,
        Description = request.Model.Description
    };

    private async Task SaveCategory(Domain.Model.Category category)
    {
        await categoryRepository.Create(category);
        logger.LogInformation(ErrorMessages.CategoryCreated, category.Name);
    }
}
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class CreateCategoryCommand : IRequest<Result>
{
    public required CreateCategoryRequestDto Model { get; set; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

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
            _logger.LogError(ex, ErrorMessages.ErrorCreatingCategory);
            return Result.Failure(ErrorMessages.ErrorCreatingCategory);   
        }
    }

    private async Task<Result> ValidateCategoryName(CreateCategoryCommand request)
    {

        var categoryExists = await _categoryRepository.CheckCategoryExistsWithName(request.Model.Name);
        if (categoryExists)
        {
            _logger.LogWarning(ErrorMessages.CategoryExists, request.Model.Name);
            return Result.Failure(ErrorMessages.CategoryExists);
        }

        return Result.Success();
    }

    private static Domain.Model.Category CreateCategoryEntity(CreateCategoryCommand request)
    {
        return new Domain.Model.Category
        {
            Name = request.Model.Name,
            Description = request.Model.Description,
        };
    }

    private async Task SaveCategory(Domain.Model.Category category)
    {
        await _categoryRepository.Create(category);
        _logger.LogInformation(ErrorMessages.CategoryCreated, category.Name);
    }
}
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class CreateCategoryCommand : IRequest<Result>
{
    public required CreateCategoryRequestDto CreateCategoryRequestDto { get; set; }
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
            _logger.LogError(ex, "Error creating category");
            return Result.Failure("An unexpected error occurred while creating the category");   
        }
    }

    private async Task<Result> ValidateCategoryName(CreateCategoryCommand request)
    {

        var categoryExists = await _categoryRepository.CheckCategoryExistsWithName(request.CreateCategoryRequestDto.Name);
        if (categoryExists)
        {
            _logger.LogWarning("Category already exists with name: {CategoryName}", request.CreateCategoryRequestDto.Name);
            return Result.Failure("Category already exists");
        }

        return Result.Success();
    }

    private static Domain.Model.Category CreateCategoryEntity(CreateCategoryCommand request)
    {
        return new Domain.Model.Category
        {
            Name = request.CreateCategoryRequestDto.Name,
            Description = request.CreateCategoryRequestDto.Description,
        };
    }

    private async Task SaveCategory(Domain.Model.Category category)
    {
        await _categoryRepository.Create(category);
        _logger.LogInformation("Category created successfully: {CategoryName}", category.Name);
    }
}
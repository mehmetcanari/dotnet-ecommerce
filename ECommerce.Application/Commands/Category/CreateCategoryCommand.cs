using Amazon.Runtime.Internal;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

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
            var categoryExist = await _categoryRepository.CheckCategoryExistsWithName(request.CreateCategoryRequestDto.Name);
            if (categoryExist)
            {
                return Result.Failure("Category already exists");
            }

            var category = new ECommerce.Domain.Model.Category
            {
                Name = request.CreateCategoryRequestDto.Name,
                Description = request.CreateCategoryRequestDto.Description,
            };

            await _categoryRepository.Create(category);
            _logger.LogInformation("Category created successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Result.Failure("An unexpected error occurred while creating the category");   
        }
    }
}
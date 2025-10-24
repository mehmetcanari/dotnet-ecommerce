using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class DeleteCategoryCommand : IRequest<Result>
{
    public required Guid Id { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryResult = await ValidateAndGetCategory(request);
            if (categoryResult.IsFailure && categoryResult.Message is not null)
                return Result.Failure(categoryResult.Message);

            if (categoryResult.Data is null)
            {
                return Result.Failure(ErrorMessages.CategoryNotFound);
            }

            DeleteCategory(categoryResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorDeletingCategory, request.Id);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(DeleteCategoryCommand request)
    {
        var category = await _categoryRepository.GetById(request.Id);
        if (category == null)
        {
            _logger.LogWarning(ErrorMessages.CategoryNotFound, request.Id);
            return Result<Domain.Model.Category>.Failure(ErrorMessages.CategoryNotFound);
        }

        return Result<Domain.Model.Category>.Success(category);
    }

    private void DeleteCategory(Domain.Model.Category category)
    {
        _categoryRepository.Delete(category);
        _logger.LogInformation(ErrorMessages.CategoryDeleted, category.Id, category.Name);
    }
}
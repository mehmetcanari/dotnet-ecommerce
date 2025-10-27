using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Category;

public class GetCategoryByIdQuery(Guid id) : IRequest<Result<CategoryResponseDto>>
{
    public readonly Guid CategoryId = id;
}

public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, ILogService logger, ICacheService cacheService) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponseDto>>
{
    private const int CacheExpirationMinutes = 60;

    public async Task<Result<CategoryResponseDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedCategory = await cacheService.GetAsync<CategoryResponseDto>(string.Format(CacheKeys.CategoryById, request.CategoryId));
            if (cachedCategory != null)
                return Result<CategoryResponseDto>.Success(cachedCategory);

            var category = await categoryRepository.GetById(request.CategoryId, cancellationToken);

            if (category == null)
                return Result<CategoryResponseDto>.Failure(ErrorMessages.CategoryNotFound);

            var categoryResponseDto = MapToResponseDto(category);
            await CacheCategory(request.CategoryId, categoryResponseDto);
    
            return Result<CategoryResponseDto>.Success(categoryResponseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.CategoryNotFound, request.CategoryId);
            return Result<CategoryResponseDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task CacheCategory(Guid categoryId, CategoryResponseDto categoryDto)
    {
        var expirationTime = TimeSpan.FromMinutes(CacheExpirationMinutes);
        await cacheService.SetAsync(string.Format(CacheKeys.CategoryById, categoryId), categoryDto, expirationTime);
    }

    private CategoryResponseDto MapToResponseDto(Domain.Model.Category category) => new CategoryResponseDto
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
    };
}
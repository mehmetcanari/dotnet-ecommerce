using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.Category;
using ECommerce.Shared.Enum;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Category;

public class GetCategoryByIdQuery(Guid id) : IRequest<Result<CategoryResponseDto>>
{
    public readonly Guid CategoryId = id;
}

public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, ILogService logger, ICacheService cacheService) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponseDto>>
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(30);

    public async Task<Result<CategoryResponseDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedItem = await cacheService.GetAsync<CategoryResponseDto>($"{CacheKeys.CategoryId}_{request.CategoryId}", cancellationToken);
            if (cachedItem is not null)
                return Result<CategoryResponseDto>.Success(cachedItem);

            var category = await categoryRepository.GetById(request.CategoryId, cancellationToken);
            if (category == null)
                return Result<CategoryResponseDto>.Failure(ErrorMessages.CategoryNotFound);

            var categoryResponseDto = MapToResponseDto(category);
            await cacheService.SetAsync($"{CacheKeys.CategoryId}_{request.CategoryId}", categoryResponseDto, CacheExpirationType.Absolute, _ttl, cancellationToken);

            return Result<CategoryResponseDto>.Success(categoryResponseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.CategoryNotFound, request.CategoryId);
            return Result<CategoryResponseDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private CategoryResponseDto MapToResponseDto(Domain.Model.Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
    };
}
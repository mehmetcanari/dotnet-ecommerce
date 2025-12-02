using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.Category;
using ECommerce.Shared.Enum;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Category;

public class GetAllCategoriesQuery(QueryPagination pagination) : IRequest<Result<List<CategoryResponseDto>>>
{
    public readonly QueryPagination Pagination = pagination;
}

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, ILogService logService, ICacheService cache) : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryResponseDto>>>
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(30);

    public async Task<Result<List<CategoryResponseDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheItems = await cache.GetAsync<List<CategoryResponseDto>>(CacheKeys.Category, cancellationToken);
            if (cacheItems is { Count: > 0 })
                return Result<List<CategoryResponseDto>>.Success(cacheItems);

            var categories = await categoryRepository.Read(request.Pagination.Page, request.Pagination.PageSize, cancellationToken);
            if (categories.Count == 0)
                return Result<List<CategoryResponseDto>>.Failure(ErrorMessages.CategoryNotFound);

            var response = categories.Select(MapToResponseDto).ToList();
            await cache.SetAsync(CacheKeys.Category, response, CacheExpirationType.Sliding, _ttl, cancellationToken);

            return Result<List<CategoryResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ex.Message);
            return Result<List<CategoryResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private CategoryResponseDto MapToResponseDto(Domain.Model.Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description
    };
}

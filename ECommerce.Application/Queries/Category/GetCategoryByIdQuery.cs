using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Category;

public class GetCategoryByIdQuery : IRequest<Result<CategoryResponseDto>>
{
    public required Guid CategoryId { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponseDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const int CacheExpirationMinutes = 60;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IProductRepository productRepository, ILoggingService logger, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryResponseDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedCategory = await GetCachedCategory(request.CategoryId);
            if (cachedCategory != null)
                return Result<CategoryResponseDto>.Success(cachedCategory);

            var category = await _categoryRepository.GetCategoryById(request.CategoryId);

            if (category == null)
                return Result<CategoryResponseDto>.Failure(ErrorMessages.CategoryNotFound);

            var categoryResponseDto = MapToResponseDto(category);
            await CacheCategory(request.CategoryId, categoryResponseDto);
    
            return Result<CategoryResponseDto>.Success(categoryResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.CategoryNotFound, request.CategoryId);
            return Result<CategoryResponseDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<CategoryResponseDto?> GetCachedCategory(Guid categoryId)
    {
        return await _cacheService.GetAsync<CategoryResponseDto>(string.Format(CacheKeys.CategoryById, categoryId));
    }

    private async Task CacheCategory(Guid categoryId, CategoryResponseDto categoryDto)
    {
        var expirationTime = TimeSpan.FromMinutes(CacheExpirationMinutes);
        await _cacheService.SetAsync(string.Format(CacheKeys.CategoryById, categoryId), categoryDto, expirationTime);
    }

    private static CategoryResponseDto MapToResponseDto(Domain.Model.Category category) => new CategoryResponseDto
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
    };
}
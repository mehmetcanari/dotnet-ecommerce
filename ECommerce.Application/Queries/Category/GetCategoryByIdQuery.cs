using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class GetCategoryByIdQuery : IRequest<Result<CategoryResponseDto>>
{
    public required int CategoryId { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponseDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const string CategoryCacheKey = "category:{0}";
    private const int CacheExpirationMinutes = 60;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, ILoggingService logger, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryResponseDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(CacheExpirationMinutes);
            var cachedCategory = await _cacheService.GetAsync<CategoryResponseDto>(string.Format(CategoryCacheKey, request.CategoryId));
            if (cachedCategory != null)
            {
                return Result<CategoryResponseDto>.Success(cachedCategory);
            }

            var category = await _categoryRepository.GetCategoryById(request.CategoryId);
            if (category == null)
            {
                return Result<CategoryResponseDto>.Failure("Category not found");
            }

            var categoryResponseDto = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                Products = category.Products.Select(p => new ProductResponseDto
                    {
                        ProductName = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        DiscountRate = p.DiscountRate,
                        ImageUrl = p.ImageUrl,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId
                    })
                    .ToList()
            };

            await _cacheService.SetAsync(string.Format(CategoryCacheKey, request.CategoryId), categoryResponseDto, expirationTime);

            _logger.LogInformation("Category retrieved successfully");
            return Result<CategoryResponseDto>.Success(categoryResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", request.CategoryId);
            return Result<CategoryResponseDto>.Failure("An unexpected error occurred while retrieving the category");
        }
    }
}
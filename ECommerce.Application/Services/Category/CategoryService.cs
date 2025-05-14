using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Application.Services.Category;

public class CategoryService : ICategoryService
{
    private const string CategoryCacheKey = "category:{0}";
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, ILoggingService logger, ICacheService cacheService, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result> CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            if (categories.Any(c => c.Name == request.Name))
            {
                return Result.Failure("Category already exists");
            }

            var category = new Domain.Model.Category
            {
                Name = request.Name,
                Description = request.Description
            };

            await _categoryRepository.Create(category);
            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category created successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            _categoryRepository.Delete(category);
            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category deleted successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
                           ?? throw new Exception("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category updated successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<CategoryResponseDto>> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedCategory = await _cacheService.GetAsync<CategoryResponseDto>(string.Format(CategoryCacheKey, categoryId));
            if (cachedCategory != null)
            {
                return Result<CategoryResponseDto>.Success(cachedCategory);
            }

            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
                           ?? throw new Exception("Category not found");

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

            await _cacheService.SetAsync(string.Format(CategoryCacheKey, categoryId), categoryResponseDto, expirationTime);

            _logger.LogInformation("Category retrieved successfully");
            return Result<CategoryResponseDto>.Success(categoryResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id");
            return Result<CategoryResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task CategoryCacheInvalidateAsync()
    {   
        try
        {
            var categories = await _categoryRepository.Read();
            foreach (var category in categories)
            {
                await _cacheService.RemoveAsync(string.Format(CategoryCacheKey, category.CategoryId));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating category cache");
            throw;
        }
    }
}
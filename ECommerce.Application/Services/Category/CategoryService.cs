using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.Extensions.Logging;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;
    private readonly ICacheService _cacheService;
    private const string CategoryCacheKey = "category:{0}";
    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger, ICacheService cacheService)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            if (categories.Any(c => c.Name == request.Name))
            {
                throw new Exception("Category already exists");
            }

            Category category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            _logger.LogInformation("Category created successfully");
            await _categoryRepository.Create(category);
            await CategoryCacheInvalidateAsync();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task<Category> DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
            ?? throw new Exception("Category not found");

            _logger.LogInformation("Category deleted successfully");
            await _categoryRepository.Delete(categoryId);
            await CategoryCacheInvalidateAsync();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            throw;
        }
    }

    public async Task<Category> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
            ?? throw new Exception("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Category updated successfully");
            await _categoryRepository.Update(category);
            await CategoryCacheInvalidateAsync();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            throw;
        }
    }

    public async Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedCategory = await _cacheService.GetAsync<CategoryResponseDto>(string.Format(CategoryCacheKey, categoryId));
            if (cachedCategory != null)
            {
                return cachedCategory;
            }

            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
            ?? throw new Exception("Category not found");

            CategoryResponseDto categoryResponseDto = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                Products = category.Products?.Select(p => new ProductResponseDto
                {
                    ProductName = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountRate = p.DiscountRate,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId
            })
            .ToList() ?? new List<ProductResponseDto>()};

            await _cacheService.SetAsync(string.Format(CategoryCacheKey, categoryId), categoryResponseDto, expirationTime);

            _logger.LogInformation("Category retrieved successfully");
            return categoryResponseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id");
            throw;
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
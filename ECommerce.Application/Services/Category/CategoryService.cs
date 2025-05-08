using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;

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

    public async Task CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            if (categories.Any(c => c.Name == request.Name))
            {
                throw new Exception("Category already exists");
            }

            Domain.Model.Category category = new Domain.Model.Category
            {
                Name = request.Name,
                Description = request.Description
            };

            await _categoryRepository.Create(category);
            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
            {
                throw new Exception("Category not found");
            }

            _categoryRepository.Delete(category);
            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            throw;
        }
    }

    public async Task UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request)
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
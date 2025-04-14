using ECommerce.Application.DTO.Request.Category;
using ECommerce.Domain.Model;
using Microsoft.Extensions.Logging;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
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
            return await _categoryRepository.Create(category);
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
            return await _categoryRepository.Delete(categoryId);
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
            return await _categoryRepository.Update(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            throw;
        }
    }

    public async Task<Category> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var categories = await _categoryRepository.Read();
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId) 
            ?? throw new Exception("Category not found");

            _logger.LogInformation("Category retrieved successfully");
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id");
            throw;
        }
    }
}
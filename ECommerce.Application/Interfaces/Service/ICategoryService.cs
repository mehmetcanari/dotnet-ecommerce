using ECommerce.Domain.Model;
using ECommerce.Application.DTO.Request.Category;

public interface ICategoryService
{
    Task<Category> CreateCategoryAsync(CreateCategoryRequestDto request);
    Task<Category> DeleteCategoryAsync(int categoryId);
    Task<Category> GetCategoryByIdAsync(int categoryId);
    Task<Category> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request);
}
using ECommerce.Domain.Model;
using ECommerce.Application.DTO.Request.Category;

public interface ICategoryService
{
    Task CreateCategoryAsync(CreateCategoryRequestDto request);
    Task DeleteCategoryAsync(int categoryId);
    Task UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request);
    Task CategoryCacheInvalidateAsync();
    Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId);
}
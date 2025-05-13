using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;

namespace ECommerce.Application.Abstract.Service;

public interface ICategoryService
{
    Task CreateCategoryAsync(CreateCategoryRequestDto request);
    Task DeleteCategoryAsync(int categoryId);
    Task UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request);
    Task CategoryCacheInvalidateAsync();
    Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId);
}
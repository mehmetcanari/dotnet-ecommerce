using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface ICategoryService
{
    Task<Result> CreateCategoryAsync(CreateCategoryRequestDto request);
    Task<Result> DeleteCategoryAsync(int categoryId);
    Task<Result> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request);
    Task CategoryCacheInvalidateAsync();
}
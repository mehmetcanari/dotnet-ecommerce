using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;

namespace ECommerce.Application.Interfaces.Service;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetProductWithIdAsync(int requestId);
    Task AddProductAsync(ProductCreateRequestDto productCreateRequest);
    Task UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequest);
    Task DeleteProductAsync(int id);
}
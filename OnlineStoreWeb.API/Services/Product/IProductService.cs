using OnlineStoreWeb.API.DTO.Request.Product;
using OnlineStoreWeb.API.DTO.Response.Product;

namespace OnlineStoreWeb.API.Services.Product;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetProductWithIdAsync(int requestId);
    Task AddProductAsync(ProductCreateRequestDto productCreateRequest);
    Task UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequest);
    Task DeleteProductAsync(int id);
}
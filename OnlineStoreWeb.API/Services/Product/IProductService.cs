using OnlineStoreWeb.API.DTO.Request.Product;
using OnlineStoreWeb.API.DTO.Response.Product;

namespace OnlineStoreWeb.API.Services.Product;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetProductWithIdAsync(int requestId);
    Task AddProductAsync(ProductCreateDto productCreate);
    Task UpdateProductAsync(int id, ProductUpdateDto productUpdate);
    Task DeleteProductAsync(int id);
}
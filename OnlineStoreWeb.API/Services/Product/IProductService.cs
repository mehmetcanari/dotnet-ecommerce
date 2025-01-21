using OnlineStoreWeb.API.DTO.Product;

namespace OnlineStoreWeb.API.Services.Product;

public interface IProductService
{
    Task<List<Model.Product>> GetAllProductsAsync();
    Task<Model.Product> GetProductWithIdAsync(int requestID);
    Task AddProductAsync(CreateProductDto createProduct);
    Task UpdateProductAsync(int id, UpdateProductDto updateProduct);
    Task DeleteProductAsync(int id);
}
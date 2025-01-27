using OnlineStoreWeb.API.DTO.Product;

namespace OnlineStoreWeb.API.Services.Product;

public interface IProductService
{
    Task<List<Model.Product>> GetAllProductsAsync();
    Task<Model.Product> GetProductWithIdAsync(int requestId);
    Task AddProductAsync(ProductCreateDto productCreate);
    Task UpdateProductAsync(int id, ProductUpdateDto productUpdate);
    Task DeleteProductAsync(int id);
}
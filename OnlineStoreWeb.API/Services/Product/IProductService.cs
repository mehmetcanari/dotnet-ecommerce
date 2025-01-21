using OnlineStoreWeb.API.DTO.Product;

namespace OnlineStoreWeb.API.Services.Product;

public interface IProductService
{
    Task<List<Model.Product>> GetAllProductsAsync();
    Task<Model.Product> GetProductWithIdAsync(ViewProductDto viewProductDto);
    Task AddProductAsync(CreateProductDto createProduct);
    Task UpdateProductAsync(UpdateProductDto updateProduct);
    Task DeleteProductAsync(int id);
}
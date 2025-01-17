public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductWithIdAsync(int id);
    Task AddProductAsync(CreateProductDto createProductDto);
    Task UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task DeleteProductAsync(int id);
}
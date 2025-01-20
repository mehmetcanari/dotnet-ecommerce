public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductWithIdAsync(int id);
    Task AddProductAsync(CreateProductDto createProduct);
    Task UpdateProductAsync(UpdateProductDto updateProduct);
    Task DeleteProductAsync(int id);
}
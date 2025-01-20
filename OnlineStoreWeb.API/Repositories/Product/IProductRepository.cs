public interface IProductRepository
{
    Task<List<Product>> Get();
    Task Add(Product product);
    Task Update(Product product);
    Task Delete(Product product);
}
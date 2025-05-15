using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IProductRepository
{
    Task Create(Product product);
    Task<List<Product>> Read();
    Task<Product?> GetProductById(int id);
    Task<bool> CheckProductExistsWithName(string name);
    void Update(Product product);
    void Delete(Product product);
}
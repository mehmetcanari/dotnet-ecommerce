using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IProductRepository
{
    Task Create(Product product);
    Task<List<Product>> Read();
    void Update(Product product);
    void Delete(Product product);
}
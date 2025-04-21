using ECommerce.Domain.Model;
namespace ECommerce.Application.Interfaces.Repository;

public interface IProductRepository
{
    Task Create(Product product);
    Task<List<Product>> Read();
    void Update(Product product);
    void Delete(Product product);
}
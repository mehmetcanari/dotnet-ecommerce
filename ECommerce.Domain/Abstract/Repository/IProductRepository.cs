using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository
{
    public interface IProductRepository
    {
        Task Create(Product product, CancellationToken cancellationToken = default);
        Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<Product> GetProductById(int id, CancellationToken cancellationToken = default);
        Task<bool> CheckProductExistsWithName(string name, CancellationToken cancellationToken = default);
        Task Update(Product product, CancellationToken cancellationToken = default);
        Task Delete(Product product, CancellationToken cancellationToken = default);
        Task DeleteById(int id, CancellationToken cancellationToken = default);
        Task<List<Product>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task UpdateStock(int productId, int newStock, CancellationToken cancellationToken = default);
    }
}
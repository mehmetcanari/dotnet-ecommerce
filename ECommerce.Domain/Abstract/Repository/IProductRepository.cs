using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository
{
    public interface IProductRepository
    {
        Task Create(Product product, CancellationToken cancellationToken = default);
        Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<Product> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<bool> CheckExistsWithName(string name, CancellationToken cancellationToken = default);
        Task Update(Product product, CancellationToken cancellationToken = default);
        Task Delete(Product product, CancellationToken cancellationToken = default);
        Task UpdateStock(Guid productId, int newStock, CancellationToken cancellationToken = default);
    }
}
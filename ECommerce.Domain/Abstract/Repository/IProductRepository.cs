using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository
{
    public interface IProductRepository
    {
        Task Create(Product product);
        Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50);
        Task<Product?> GetProductById(int id);
        Task<bool> CheckProductExistsWithName(string name);
        Task Update(Product product);
        Task Delete(Product product);
        Task DeleteById(int id);
        Task<List<Product>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 50);
        Task UpdateStock(int productId, int newStock);
    }
}
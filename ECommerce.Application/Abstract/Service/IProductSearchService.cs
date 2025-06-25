using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;
public interface IProductSearchService
{
    Task<Result> IndexProductAsync(Product product);
    Task<Result> DeleteProductAsync(string productId);
    Task<Result<List<Product>>> SearchProductsAsync(string query, int page = 1, int pageSize = 10);
    Task<Result> UpdateProductAsync(Product product);
    Task<Result> BulkIndexProductsAsync(IEnumerable<Product> products);
    Task<Result> CreateProductIndexWithNGramAsync();
}
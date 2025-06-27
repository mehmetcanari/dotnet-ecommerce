using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;
public interface IProductSearchService
{
    Task<Result> IndexProductAsync(Product product);
    Task<Result> DeleteProductAsync(string productId);
    Task<Result<List<ProductResponseDto>>> SearchProductsAsync(string query, int page = 1, int pageSize = 10);
    Task<Result> UpdateProductAsync(Product product);
    Task<Result> BulkIndexProductsAsync(IEnumerable<Product> products);
    Task<Result> CreateProductIndexWithNGramAsync();
    Task<Result> InitializeIndexAsync(IEnumerable<Product> products);
    Task<Result> ReindexAllProductsAsync(IEnumerable<Product> products);
}
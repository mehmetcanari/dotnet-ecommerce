using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using Elastic.Clients.Elasticsearch;

namespace ECommerce.Application.Abstract.Service;
public interface IProductSearchService
{
    Task<Utility.Result> IndexProductAsync(Product product);
    Task<Utility.Result> DeleteProductAsync(string productId);
    Task<SearchResponse<Product>> SearchProductsAsync(string query, int page = 1, int pageSize = 10);
    Task<Utility.Result> UpdateProductAsync(Product product);
    Task<Utility.Result> BulkIndexProductsAsync(IEnumerable<Product> products);
    Task<Utility.Result> CreateProductIndexWithNGramAsync();
    Task<Utility.Result> ReindexAllProductsAsync(IEnumerable<Product> products);
}
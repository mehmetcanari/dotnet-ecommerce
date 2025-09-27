using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using Elastic.Clients.Elasticsearch;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Services.Search.Product;
public class ProductSearchService : IProductSearchService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILoggingService _logger;
    private const string ProductIndexName = "products";

    public ProductSearchService(
    ElasticsearchClient elasticClient,
    ILoggingService loggingService)
    {
        _elasticClient = elasticClient;
        _logger = loggingService;
    }

    public async Task<Result> IndexProductAsync(Domain.Model.Product product)
    {
        try
        {
            if (product == null)
            {
                return Result.Failure("Product cannot be null");
            }

            var existsResponse = await _elasticClient.Indices.ExistsAsync(ProductIndexName);
            if (!existsResponse.IsValidResponse || (existsResponse.ApiCallDetails?.HttpStatusCode != 200))
            {
                _logger.LogInformation("Index {IndexName} does not exist, creating it", ProductIndexName);
                var createResult = await CreateProductIndexWithNGramAsync();
                if (!createResult.IsSuccess)
                {
                    return Result.Failure($"Failed to create index: {createResult.Error}");
                }
            }

            var response = await _elasticClient.IndexAsync(product, ProductIndexName, i => i.Id(product.ProductId.ToString()));
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to index product: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while indexing product: {Message}", ex.Message);
            return Result.Failure("Unexpected error while indexing product");
        }
    }

    public async Task<Result> DeleteProductAsync(string productId)
    {
        try
        {
            if (string.IsNullOrEmpty(productId))
            {
                return Result.Failure("Product ID cannot be null or empty");
            }

            var response = await _elasticClient.DeleteAsync(ProductIndexName, productId);
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to delete product: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            return Result.Failure("Unexpected error while deleting product");
        }
    }

    public async Task<SearchResponse<Domain.Model.Product>> SearchProductsAsync(string query, int page = 1, int pageSize = 10)
    {
        var searchResponse = await _elasticClient.SearchAsync<Domain.Model.Product>(s => s
            .Indices(ProductIndexName)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.Match(m => m
                            .Field(f => f.Name)
                            .Query(query)
                            .Boost(3.0f)
                        ),
                        sh => sh.Fuzzy(fz => fz
                            .Field(f => f.Name)
                            .Value(query)
                            .Fuzziness(new Fuzziness(2))   
                            .Transpositions(true)
                            .Boost(3.0f)
                        ),
                        sh => sh.Prefix(p => p
                            .Field(f => f.Name)
                            .Value(query)
                            .CaseInsensitive(true)
                            .Boost(2.0f)
                        ),
                        sh => sh.Wildcard(w => w
                            .Field(f => f.Name)
                            .Value($"*{query}*")
                            .CaseInsensitive(true)
                            .Boost(1.5f)
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            )
            .From((page - 1) * pageSize)
            .Size(pageSize)
        );

        return searchResponse;
    }

    public async Task<Result> UpdateProductAsync(Domain.Model.Product product)
    {
        try
        {
            if (product == null)
            {
                return Result.Failure("Product cannot be null");
            }

            var response = await _elasticClient.UpdateAsync<Domain.Model.Product, Domain.Model.Product>(ProductIndexName, product.ProductId.ToString(), u => u.Doc(product));
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to update product: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            return Result.Failure("Unexpected error while updating product");
        }
    }

    public async Task<Result> DeleteAllProductsFromIndexAsync()
    {
        try
        {
            var response = await _elasticClient.DeleteByQueryAsync<Domain.Model.Product>(ProductIndexName, d => d.Query(q => q.MatchAll(_ => { })));
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to delete all products from index: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            _logger.LogInformation("All products deleted from Elasticsearch index");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting all products from index: {Message}", ex.Message);
            return Result.Failure("Unexpected error while deleting all products from index");
        }
    }

    public async Task<Result> BulkIndexProductsAsync(IEnumerable<Domain.Model.Product> products)
    {
        try
        {
            if (products == null || !products.Any())
            {
                return Result.Failure("Product list cannot be null or empty");
            }

            var response = await _elasticClient.BulkAsync(b => b
                .Index(ProductIndexName)
                .IndexMany(products));

            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to bulk index products: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            _logger.LogInformation("Successfully bulk indexed {Count} products", products.Count());
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while bulk indexing products: {Message}", ex.Message);
            return Result.Failure("Unexpected error while bulk indexing products");
        }
    }

    public async Task<Result> ReindexAllProductsAsync(IEnumerable<Domain.Model.Product> products)
    {
        try
        {
            var deleteResult = await DeleteAllProductsFromIndexAsync();
            if (!deleteResult.IsSuccess)
            {
                return Result.Failure($"Failed to clear index before reindexing: {deleteResult.Error}");
            }

            var bulkResult = await BulkIndexProductsAsync(products);
            if (!bulkResult.IsSuccess)
            {
                return Result.Failure($"Failed to bulk index products: {bulkResult.Error}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reindexing products: {Message}", ex.Message);
            return Result.Failure("Unexpected error while reindexing products");
        }
    }

    public async Task<Result> CreateProductIndexWithNGramAsync()
    {
        try
        {
            var existsResponse = await _elasticClient.Indices.ExistsAsync(ProductIndexName);
            if (existsResponse.IsValidResponse && existsResponse.ApiCallDetails != null && existsResponse.ApiCallDetails.HttpStatusCode == 200)
                return Result.Success();

            // Mapping olmadan, otomatik mapping ile index oluştur
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(ProductIndexName);
            _logger.LogInformation("Index creation response: {@Response}", createIndexResponse);
            return createIndexResponse.IsValidResponse ? Result.Success() : Result.Failure($"Failed to create index: {createIndexResponse.ElasticsearchServerError?.Error?.Reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product index: {Message}", ex.Message);
            return Result.Failure("Unexpected error while creating product index");
        }
    }
}
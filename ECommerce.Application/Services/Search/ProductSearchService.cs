using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Services.Search;
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

            var response = await _elasticClient.IndexAsync(product, ProductIndexName);
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

    public async Task<Result<List<Domain.Model.Product>>> SearchProductsAsync(string query, int page = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return Result<List<Domain.Model.Product>>.Failure("Query cannot be null or empty");
            }

            _logger.LogInformation("Elasticsearch query: {Query}", query.ToLower());
            _logger.LogInformation("Elasticsearch index: {Index}", ProductIndexName);
            _logger.LogInformation("Elasticsearch client endpoint: {Endpoint}", _elasticClient.ElasticsearchClientSettings?.NodePool?.Nodes?.FirstOrDefault()?.Uri?.ToString() ?? "unknown");

            var searchResponse = await _elasticClient.SearchAsync<Domain.Model.Product>(s => s
                .Indices(ProductIndexName)
                .Query(q => q
                    .Match(m => m
                        .Field("name")
                        .Query(query.ToLower())
                    )
                )
                .From((page - 1) * pageSize)
                .Size(pageSize));

            _logger.LogInformation("Elasticsearch response: {@Response}", searchResponse);

            if (searchResponse == null)
            {
                _logger.LogError(null, "Elasticsearch searchResponse is null!");
                return Result<List<Domain.Model.Product>>.Failure("Elasticsearch response is null");
            }

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogError(null, "Elasticsearch search failed: {Error}", searchResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error");
                return Result<List<Domain.Model.Product>>.Failure("Elasticsearch search failed");
            }

            if (searchResponse.Documents == null || !searchResponse.Documents.Any())
            {
                _logger.LogInformation("No products found for query: {Query}", query);
                return Result<List<Domain.Model.Product>>.Success(new List<Domain.Model.Product>());
            }

            return Result<List<Domain.Model.Product>>.Success(searchResponse.Documents.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while searching products: {Message}", ex.Message);
            return Result<List<Domain.Model.Product>>.Failure("Unexpected error while searching products");
        }
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

            // Önce index'teki tüm ürünleri sil
            var deleteResult = await DeleteAllProductsFromIndexAsync();
            if (!deleteResult.IsSuccess)
            {
                return Result.Failure($"Failed to clear index before bulk indexing: {deleteResult.Error}");
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

    public async Task<Result> CreateProductIndexWithNGramAsync()
    {
        try
        {
            var existsResponse = await _elasticClient.Indices.ExistsAsync(ProductIndexName);
            if (existsResponse.IsValidResponse && existsResponse.ApiCallDetails != null && existsResponse.ApiCallDetails.HttpStatusCode == 200)
                return Result.Success();

            var createIndexResponse = await _elasticClient.Indices.CreateAsync(ProductIndexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("ngram_analyzer", ca => ca
                                .Tokenizer("ngram_tokenizer")
                                .Filter(new[] { "lowercase" })
                            )
                        )
                        .Tokenizers(tz => tz
                            .NGram("ngram_tokenizer", ng => ng
                                .MinGram(3)
                                .MaxGram(10)
                                .TokenChars(new[] { Elastic.Clients.Elasticsearch.Analysis.TokenChar.Letter, Elastic.Clients.Elasticsearch.Analysis.TokenChar.Digit })
                            )
                        )
                    )
                )
                .Mappings(ms => ms
                    .Properties(new Properties
                    {
                        { "name", new TextProperty { Analyzer = "ngram_analyzer" } }
                    })
                )
            );
            return createIndexResponse.IsValidResponse ? Result.Success() : Result.Failure("Failed to create index with ngram analyzer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product index: {Message}", ex.Message);
            return Result.Failure("Unexpected error while creating product index");
        }
    }
}
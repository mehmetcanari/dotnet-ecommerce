using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using Elastic.Clients.Elasticsearch;
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

    public Task<Result> IndexProductAsync(Domain.Model.Product product)
    {
        try
        {
            if (product == null)
            {
                return Task.FromResult(Result.Failure("Product cannot be null"));
            }

            return _elasticClient.IndexAsync(product, idx => idx.Index(ProductIndexName))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        return Result.Failure($"Failed to index product: {task.Exception?.Message}");
                    }
                    
                    return Result.Success();
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while indexing product: {Message}", ex.Message);
            return Task.FromResult(Result.Failure("Unexpected error while indexing product"));
        }
    }

    public Task<Result> DeleteProductAsync(string productId)
    {
        try
        {
            if (string.IsNullOrEmpty(productId))
            {
                return Task.FromResult(Result.Failure("Product ID cannot be null or empty"));
            }

            return _elasticClient.DeleteAsync<Domain.Model.Product>(productId, idx => idx.Index(ProductIndexName))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        return Result.Failure($"Failed to delete product: {task.Exception?.Message}");
                    }
                    
                    return Result.Success();
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            return Task.FromResult(Result.Failure("Unexpected error while deleting product"));
        }
    }

    public Task<Result<List<Domain.Model.Product>>> SearchProductsAsync(string query, int page = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return Task.FromResult(Result<List<Domain.Model.Product>>.Failure("Query cannot be null or empty"));
            }

            var searchResponse = _elasticClient.SearchAsync<Domain.Model.Product>(s => s
                .Indices(ProductIndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(query)))
                .From((page - 1) * pageSize)
                .Size(pageSize));

            return searchResponse.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return Result<List<Domain.Model.Product>>.Failure($"Failed to search products: {task.Exception?.Message}");
                }

                return Result<List<Domain.Model.Product>>.Success(task.Result.Documents.ToList());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while searching products: {Message}", ex.Message);
            return Task.FromResult(Result<List<Domain.Model.Product>>.Failure("Unexpected error while searching products"));
        }
    }

    public Task<Result> UpdateProductAsync(Domain.Model.Product product)
    {
        try
        {
            if (product == null)
            {
                return Task.FromResult(Result.Failure("Product cannot be null"));
            }

            var updateRequest = new UpdateRequest<Domain.Model.Product, Domain.Model.Product>(ProductIndexName, product.ProductId.ToString())
            {
                Doc = product
            };
            return _elasticClient.UpdateAsync(updateRequest)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        return Result.Failure($"Failed to update product: {task.Exception?.Message}");
                    }
                    
                    return Result.Success();
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            return Task.FromResult(Result.Failure("Unexpected error while updating product"));
        }
    }

    public Task<Result> BulkIndexProductsAsync(IEnumerable<Domain.Model.Product> products)
    {
        try
        {
            if (products == null || !products.Any())
            {
                return Task.FromResult(Result.Failure("Product list cannot be null or empty"));
            }

            var bulkResponse = _elasticClient.BulkAsync(b => b
                .Index(ProductIndexName)
                .IndexMany(products));

            return bulkResponse.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return Result.Failure($"Failed to bulk index products: {task.Exception?.Message}");
                }
                
                return Result.Success();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while bulk indexing products: {Message}", ex.Message);
            return Task.FromResult(Result.Failure("Unexpected error while bulk indexing products"));
        }
    }
}
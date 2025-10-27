using ECommerce.Application.Abstract;
using Elastic.Clients.Elasticsearch;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Services.Search;

public class ElasticSearchService(ElasticsearchClient elasticClient, ILogService logService) : IElasticSearchService
{
    private const string ProductIndexName = "products";

    public async Task<Result> IndexProductAsync(Domain.Model.Product product)
    {
        try
        {
            var existsResponse = await elasticClient.Indices.ExistsAsync(ProductIndexName);
            if (!existsResponse.IsValidResponse || existsResponse.ApiCallDetails?.HttpStatusCode != 200)
            {
                var createIndexResponse = await elasticClient.Indices.CreateAsync(ProductIndexName);
                logService.LogInformation("Index creation response: {@Response}", createIndexResponse);

                if (!createIndexResponse.IsValidResponse)
                    return Result.Failure($"Failed to create index: {createIndexResponse.ElasticsearchServerError?.Error?.Reason}");
            }

            var response = await elasticClient.IndexAsync(product, ProductIndexName, i => i.Id(product.Id.ToString()));
            if (!response.IsValidResponse)
                return Result.Failure($"Failed to index product: {response.ElasticsearchServerError?.Error?.Reason}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "Unexpected error while indexing product: {Message}", ex.Message);
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

            var response = await elasticClient.DeleteAsync(ProductIndexName, productId);
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to delete product: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            return Result.Failure("Unexpected error while deleting product");
        }
    }

    public async Task<SearchResponse<Domain.Model.Product>> SearchProductsAsync(string query, int page = 1, int pageSize = 10)
    {
        var searchResponse = await elasticClient.SearchAsync<Domain.Model.Product>(s => s
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

            var response = await elasticClient.UpdateAsync<Domain.Model.Product, Domain.Model.Product>(ProductIndexName, product.Id.ToString(), u => u.Doc(product));
            if (!response.IsValidResponse)
            {
                return Result.Failure($"Failed to update product: {response.ElasticsearchServerError?.Error?.Reason}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            return Result.Failure("Unexpected error while updating product");
        }
    }
}
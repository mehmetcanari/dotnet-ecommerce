using ECommerce.Application.Abstract;
using ECommerce.Shared.Constants;
using Elastic.Clients.Elasticsearch;
using ElasticResult = Elastic.Clients.Elasticsearch.Result;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Services.Elastic;

public class ElasticSearchService(ElasticsearchClient client, ILogService logger) : IElasticSearchService
{
    public async Task<Result> IndexAsync<T>(T document, string indexName, string? id = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var exists = await client.Indices.ExistsAsync(indexName, cancellationToken);

            if (!exists.Exists)
            {
                var createResponse = await client.Indices.CreateAsync(indexName, c => c.Settings(s => s.NumberOfShards(1).NumberOfReplicas(0)), cancellationToken);
                if (!createResponse.IsValidResponse)
                    return Result.Failure(ErrorMessages.IndexCreationFailure + $"{createResponse.ElasticsearchServerError?.Error?.Reason}");
            }

            IndexResponse response = string.IsNullOrEmpty(id)
                ? await client.IndexAsync(document, i => i.Index(indexName), cancellationToken)
                : await client.IndexAsync(document, i => i.Index(indexName).Id(id), cancellationToken);

            if (!response.IsValidResponse)
                return Result.Failure(ErrorMessages.IndexFailure + $"{response.ElasticsearchServerError?.Error?.Reason}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedElasticError);
        }
    }

    public async Task<Result> UpdateAsync<T>(string id, T document, string indexName, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var response = await client.UpdateAsync<T, T>(indexName, id, u => u.Doc(document), cancellationToken);
            if (!response.IsValidResponse)
                return Result.Failure(ErrorMessages.UpdateFailure + $"{response.ElasticsearchServerError?.Error?.Reason}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedElasticError);
        }
    }

    public async Task<Result> DeleteAsync<T>(string id, string indexName, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var response = await client.DeleteAsync(indexName, id, cancellationToken);
            if (!response.IsValidResponse && response.Result != ElasticResult.NotFound)
                return Result.Failure(ErrorMessages.DeleteFailure + $"{response.ElasticsearchServerError?.Error?.Reason}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedElasticError);
        }
    }

    public async Task<SearchResponse<T>> SearchAsync<T>(Func<SearchRequestDescriptor<T>, SearchRequestDescriptor<T>> descriptor, string indexName, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var response = await client.SearchAsync<T>(d => descriptor(d.Indices(indexName)), cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.SearchFailure, ex.Message);
            throw;
        }
    }
}
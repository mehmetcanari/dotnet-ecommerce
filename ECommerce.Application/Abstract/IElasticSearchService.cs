using Elastic.Clients.Elasticsearch;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Abstract;

public interface IElasticSearchService
{
    Task<Result> IndexAsync<T>(T document, string indexName, string? id = null, CancellationToken cancellationToken = default) where T : class;
    Task<Result> UpdateAsync<T>(string id, T document, string indexName, CancellationToken cancellationToken = default) where T : class;
    Task<Result> DeleteAsync<T>(string id, string indexName, CancellationToken cancellationToken = default) where T : class;
    Task<SearchResponse<T>> SearchAsync<T>(Func<SearchRequestDescriptor<T>, SearchRequestDescriptor<T>> descriptor, string indexName, CancellationToken cancellationToken = default) where T : class;
}
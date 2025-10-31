using Elastic.Clients.Elasticsearch;

namespace ECommerce.Application.Abstract;

public interface ISearchDescriptor<T> where T : class
{
    Func<SearchRequestDescriptor<T>, SearchRequestDescriptor<T>> Build(string query, int page, int pageSize);
}
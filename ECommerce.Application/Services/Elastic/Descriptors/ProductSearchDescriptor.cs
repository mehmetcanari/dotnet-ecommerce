using ECommerce.Application.Abstract;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace ECommerce.Application.Services.Elastic.Descriptors;

public class ProductSearchDescriptor : ISearchDescriptor<Domain.Model.Product>
{
    public Func<SearchRequestDescriptor<Domain.Model.Product>, SearchRequestDescriptor<Domain.Model.Product>> Build(string query, int page, int pageSize)
    {
        var from = (page - 1) * pageSize;

        return s => s
            .Query(q => q
                .Bool(b => b
                    .Should(
                        mm => mm.MultiMatch(m => m
                            .Query(query)
                            .Fields(new[]
                            {
                                Infer.Field<Domain.Model.Product>(p => p.Name, boost: 5),
                                Infer.Field<Domain.Model.Product>(p => p.Description)
                            })
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(2)
                            .PrefixLength(1)
                            .MaxExpansions(100)
                            .Operator(Operator.Or)
                        ),
                        wc => wc.Wildcard(w => w
                            .Field(f => f.Name)
                            .Value($"*{query.ToLowerInvariant()}*")
                            .CaseInsensitive(true)
                            .Boost(0.3f)
                        ),
                        mpp => mpp.MatchPhrasePrefix(m => m
                            .Field(f => f.Name)
                            .Query(query)
                            .MaxExpansions(50)
                            .Boost(0.4f)
                        ),
                        pr => pr.Prefix(p => p
                            .Field(f => f.Name)
                            .Value(query.ToLowerInvariant())
                            .Boost(0.2f)
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            )
            .From(from)
            .Size(pageSize);
    }
}

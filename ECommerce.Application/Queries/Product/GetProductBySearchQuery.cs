using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using Elastic.Clients.Elasticsearch.Core.Search;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetProductBySearchQuery(string query, int page = 1, int pageSize = 10) : IRequest<Result<List<ProductResponseDto>>>
{
    public string Query { get; set; } = query;
    public int Page { get; set; } = page;
    public int PageSize { get; set; } = pageSize;
}

public class ProductSearchQueryHandler(IElasticSearchService elasticSearchService, ILogService logger, ISearchDescriptor<Domain.Model.Product> specification) : IRequestHandler<GetProductBySearchQuery, Result<List<ProductResponseDto>>>
{
    private const string ProductIndexName = "products";

    public async Task<Result<List<ProductResponseDto>>> Handle(GetProductBySearchQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return Result<List<ProductResponseDto>>.Failure(ErrorMessages.QueryCannotBeEmpty);

            var searchDescriptor = specification.Build(request.Query, request.Page, request.PageSize);
            var response = await elasticSearchService.SearchAsync(searchDescriptor, ProductIndexName, cancellationToken);

            if (!response.IsValidResponse)
                return Result<List<ProductResponseDto>>.Failure(ErrorMessages.NoSearchResults);

            var products = MapToProductResponse(response.Hits);

            return Result<List<ProductResponseDto>>.Success(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ErrorMessages.UnexpectedElasticError);
        }
    }

    private List<ProductResponseDto> MapToProductResponse(IReadOnlyCollection<Hit<Domain.Model.Product>> hits) => hits.Where(h => h.Source is not null).Select(h => new ProductResponseDto
    {
        Id = h.Source!.Id,
        ProductName = h.Source.Name,
        Description = h.Source.Description,
        Price = h.Source.Price,
        DiscountRate = h.Source.DiscountRate,
        ImageUrl = h.Source.ImageUrl,
        StockQuantity = h.Source.StockQuantity,
        CategoryId = h.Source.CategoryId
    }).ToList();
}
using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Product;
public class ProductSearchQuery(string query, int page = 1, int pageSize = 10) : IRequest<Result<List<ProductResponseDto>>>
{
    public string Query { get; set; } = query;
    public int Page { get; set; } = page;
    public int PageSize { get; set; } = pageSize;
}
public class ProductSearchQueryHandler(IElasticSearchService elasticSearchService, ILogService logger) : IRequestHandler<ProductSearchQuery, Result<List<ProductResponseDto>>>
{
    public async Task<Result<List<ProductResponseDto>>> Handle(ProductSearchQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
                return Result<List<ProductResponseDto>>.Failure(ErrorMessages.QueryCannotBeEmpty);

            var result = await elasticSearchService.SearchProductsAsync(request.Query, request.Page, request.PageSize);

            if (result.Hits.Count == 0)
                return Result<List<ProductResponseDto>>.Success(new List<ProductResponseDto>());

            var elasticProductResponse = Result<List<ProductResponseDto>>.Success(result.Hits.Select(d => new ProductResponseDto
            {
                Id = d.Source.Id,
                ProductName = d.Source.Name,
                Description = d.Source.Description,
                Price = d.Source.Price,
                DiscountRate = d.Source.DiscountRate,
                ImageUrl = d.Source.ImageUrl,
                StockQuantity = d.Source.StockQuantity,
                CategoryId = d.Source.CategoryId
            }).ToList());

            return elasticProductResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.NoSearchResults, ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ErrorMessages.NoSearchResults);
        }
    }
}
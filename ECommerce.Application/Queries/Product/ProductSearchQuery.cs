using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Product;
public class ProductSearchQuery : IRequest<Result<List<ProductResponseDto>>>
{
    public string Query { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public ProductSearchQuery(string query, int page = 1, int pageSize = 10)
    {
        Query = query;
        Page = page;
        PageSize = pageSize;
    }
}
public class ProductSearchQueryHandler : IRequestHandler<ProductSearchQuery, Result<List<ProductResponseDto>>>
{
    private readonly IProductSearchService _productSearchService;
    private readonly ILoggingService _logger;

    public ProductSearchQueryHandler(IProductSearchService productSearchService, ILoggingService logger)
    {
        _productSearchService = productSearchService;
        _logger = logger;
    }

    public async Task<Result<List<ProductResponseDto>>> Handle(ProductSearchQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
                return Result<List<ProductResponseDto>>.Failure(ErrorMessages.QueryCannotBeEmpty);

            var result = await _productSearchService.SearchProductsAsync(request.Query, request.Page, request.PageSize);

            if (result == null || !result.Hits.Any())
                return Result<List<ProductResponseDto>>.Success(new List<ProductResponseDto>());

            var elasticProductResponse = Result<List<ProductResponseDto>>.Success(result.Hits.Select(d => new ProductResponseDto
            {
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
            _logger.LogError(ex, ErrorMessages.NoSearchResults, ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ErrorMessages.NoSearchResults);
        }
    }
}
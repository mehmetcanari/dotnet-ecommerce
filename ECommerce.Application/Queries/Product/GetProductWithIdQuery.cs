using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetProductWithIdQuery : IRequest<Result<ProductResponseDto>>
{
    public int ProductId { get; set; }
}

public class GetProductWithIdQueryHandler : IRequestHandler<GetProductWithIdQuery, Result<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _logger;
    private const string ProductCacheKey = "product:{0}";
    private const int CacheDurationInMinutes = 60;

    public GetProductWithIdQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILoggingService logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ProductResponseDto>> Handle(GetProductWithIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(CacheDurationInMinutes);
            var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(string.Format(ProductCacheKey, request.ProductId));
            if (cachedProduct != null)
            {
                return Result<ProductResponseDto>.Success(cachedProduct);
            }

            var product = await _productRepository.GetProductById(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product with id {Id} not found", request.ProductId);
                return Result<ProductResponseDto>.Failure("Product not found");
            }
            
            var productResponse = new ProductResponseDto
            {
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };

            await _cacheService.SetAsync(string.Format(ProductCacheKey, request.ProductId), productResponse, expirationTime);
            return Result<ProductResponseDto>.Success(productResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product with id: {Message}", ex.Message);
            return Result<ProductResponseDto>.Failure(ex.Message);
        }
    }
}
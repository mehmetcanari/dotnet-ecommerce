using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetProductByIdQuery : IRequest<Result<ProductResponseDto>>
{
    public required Guid Id { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _logger;
    private const int CacheDurationInMinutes = 60;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILoggingService logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(string.Format(CacheKeys.ProductById, request.Id));
            if (cachedProduct != null)
                return Result<ProductResponseDto>.Success(cachedProduct);

            var product = await _productRepository.GetProductById(request.Id);
            if (product == null)
                return Result<ProductResponseDto>.Failure(ErrorMessages.ProductNotFound);
            
            var productResponse = MapToResponseDto(product);
            await CacheProduct(request.Id, productResponse);
            
            return Result<ProductResponseDto>.Success(productResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<ProductResponseDto>.Failure(ex.Message);
        }
    }

    private async Task CacheProduct(Guid productId, ProductResponseDto productDto)
    {
        var expirationTime = TimeSpan.FromMinutes(CacheDurationInMinutes);
        await _cacheService.SetAsync(string.Format(CacheKeys.ProductById, productId), productDto, expirationTime);
    }

    private static ProductResponseDto MapToResponseDto(Domain.Model.Product product) => new ProductResponseDto
    {
        Id = product.Id,
        ProductName = product.Name,
        Description = product.Description,
        Price = product.Price,
        DiscountRate = product.DiscountRate,
        ImageUrl = product.ImageUrl,
        StockQuantity = product.StockQuantity,
        CategoryId = product.CategoryId
    };
}
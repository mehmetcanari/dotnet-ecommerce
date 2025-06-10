using ECommerce.Application.DTO.Response.Product;
using MediatR;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using Microsoft.Extensions.Logging;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Application.Queries.Product;

public class GetAllProductsQuery : IRequest<Result<List<ProductResponseDto>>>{}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductResponseDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    private const string AllProductsCacheKey = "all_products";

    public GetAllProductsQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<List<ProductResponseDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedProducts = await _cacheService.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey);
            if (cachedProducts is { Count: > 0 })
            {
                return Result<List<ProductResponseDto>>.Success(cachedProducts);
            }

            var products = await _productRepository.Read();

            if (products.Count == 0)
            {
                throw new Exception("No products found");
            }

            var productResponses = products.Select(p => new ProductResponseDto
            {
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId
            }).ToList();

            await _cacheService.SetAsync(AllProductsCacheKey, productResponses, expirationTime);
            return Result<List<ProductResponseDto>>.Success(productResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all products");
            return Result<List<ProductResponseDto>>.Failure(ex.Message);
        }
    }
}
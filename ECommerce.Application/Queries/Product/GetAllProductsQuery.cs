using ECommerce.Application.DTO.Response.Product;
using MediatR;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using Microsoft.Extensions.Logging;
using ECommerce.Domain.Abstract.Repository;
using System.Text.Json;

namespace ECommerce.Application.Queries.Product;

public class GetAllProductsQuery : IRequest<Result<List<ProductResponseDto>>>{}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductResponseDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    private const string AllProductsCacheKey = "products";
    private int _cacheDurationInMinutes = 60;

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
            var expirationTime = TimeSpan.FromMinutes(_cacheDurationInMinutes);
            var cachedProducts = await _cacheService.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey);
            if (cachedProducts is { Count: > 0 })
            {
                _logger.LogInformation("Retrieved {Count} products from cache", cachedProducts.Count);
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

            // Log the first product to verify the data structure
            if (productResponses.Any())
            {
                var firstProduct = productResponses.First();
                _logger.LogInformation("First product before caching: ProductName={ProductName}, Description={Description}", 
                    firstProduct.ProductName, firstProduct.Description);
            }

            await _cacheService.SetAsync(AllProductsCacheKey, productResponses, expirationTime);
            _logger.LogInformation("Successfully cached {Count} products", productResponses.Count);
            
            return Result<List<ProductResponseDto>>.Success(productResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all products: {Message}", ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ex.Message);
        }
    }
}
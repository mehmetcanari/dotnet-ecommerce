using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
namespace ECommerce.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;
    private readonly ICacheService _cacheService;
    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _logger = logger;
        _cacheService = cacheService;
    }
    
    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedProducts = await _cacheService.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey);
            if (cachedProducts is { Count: > 0 })
            {
                Console.WriteLine("Products found in cache, fetching from cache");
                return cachedProducts;
            }

            var products = await _productRepository.Read();

            if (products.Count == 0)
            {
                _logger.LogWarning("No products found in the database");
                return [];
            }

            var productResponseDtos = products.Select(p => new ProductResponseDto
            {
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity
            }).ToList();

            await _cacheService.SetAsync(AllProductsCacheKey, productResponseDtos, expirationTime);
            return productResponseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all products");
            throw;
        }
    }

    public async Task<ProductResponseDto> GetProductWithIdAsync(int requestId)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(string.Format(ProductCacheKey, requestId));
            if (cachedProduct != null)
                return cachedProduct;

            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == requestId) ?? throw new Exception("Product not found");
            
            var productResponse = new ProductResponseDto
            {
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity
            };

            await _cacheService.SetAsync(string.Format(ProductCacheKey, requestId), productResponse, expirationTime);
            return productResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product with id: {Message}", ex.Message);
            throw;
        }
    }

    public async Task AddProductAsync(ProductCreateRequestDto productCreateRequestRequest)
    {
        try
        {
            var products = await _productRepository.Read();
            if (products.Any(p => p.Name == productCreateRequestRequest.Name)) //Duplicate product name check
            {
                throw new Exception("Product already exists in the database");
            }

            var product = new Domain.Model.Product
            {
                Name = productCreateRequestRequest.Name,
                Description = productCreateRequestRequest.Description,
                Price = productCreateRequestRequest.Price,
                DiscountRate = productCreateRequestRequest.DiscountRate,
                ImageUrl = productCreateRequestRequest.ImageUrl,
                StockQuantity = productCreateRequestRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };
            
            if (product.DiscountRate > 0)
                product.Price -= product.Price * product.DiscountRate / 100; //Calculate discounted price
            
            await _productRepository.Create(product);
            await _cacheService.RemoveAsync(AllProductsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequestRequest)
    {
        try
        {
            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == id) ?? throw new Exception("Product not found");

            product.Name = productUpdateRequestRequest.Name;
            product.Description = productUpdateRequestRequest.Description;
            product.Price = productUpdateRequestRequest.Price;
            product.DiscountRate = productUpdateRequestRequest.DiscountRate;
            product.ImageUrl = productUpdateRequestRequest.ImageUrl;
            product.StockQuantity = productUpdateRequestRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;
            
            if (product.DiscountRate > 0)
                product.Price -= product.Price * product.DiscountRate / 100; //Calculate discounted price

            await _productRepository.Update(product);
            await _cacheService.RemoveAsync(AllProductsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == id) ?? throw new Exception("Product not found");

            await _productRepository.Delete(product);
            await _cacheService.RemoveAsync(AllProductsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }
}
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryService _categoryService;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductService(IProductRepository productRepository, ICategoryService categoryService, ILoggingService logger, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
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
                return cachedProducts;
            }

            var products = await _productRepository.Read();

            if (products.Count == 0)
            {
                throw new Exception("No products found");
            }

            var productResponseDtos = products.Select(p => new ProductResponseDto
            {
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId
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
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
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

    public async Task AddProductAsync(ProductCreateRequestDto productCreateRequest)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(productCreateRequest.CategoryId);
            var products = await _productRepository.Read();

            if (products.Any(p => p.Name == productCreateRequest.Name))
            {
                throw new Exception("Product already exists in the database");
            }

            var product = new Domain.Model.Product
            {
                Name = productCreateRequest.Name,
                Description = productCreateRequest.Description,
                Price = productCreateRequest.Price,
                DiscountRate = productCreateRequest.DiscountRate,
                ImageUrl = productCreateRequest.ImageUrl,
                StockQuantity = productCreateRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow,
                CategoryId = category.CategoryId
            };

            if (product.DiscountRate > 0)
                product.Price -= product.Price * product.DiscountRate / 100;

            await _cacheService.RemoveAsync(AllProductsCacheKey);
            await _productRepository.Create(product);

            _logger.LogInformation("Product created successfully: {Product}", product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding product: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequest)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(productUpdateRequest.CategoryId);
            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == id) ?? throw new Exception("Product not found");

            product.Name = productUpdateRequest.Name;
            product.Description = productUpdateRequest.Description;
            product.Price = productUpdateRequest.Price;
            product.DiscountRate = productUpdateRequest.DiscountRate;
            product.ImageUrl = productUpdateRequest.ImageUrl;
            product.StockQuantity = productUpdateRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;
            product.CategoryId = category.CategoryId;

            if (product.DiscountRate > 0)
                product.Price -= product.Price * product.DiscountRate / 100;

            await _productRepository.Update(product);
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            _logger.LogInformation("Product updated successfully: {Product}", product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            throw;
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

            _logger.LogInformation("Product deleted successfully: {Product}", product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw;
        }
    }
}
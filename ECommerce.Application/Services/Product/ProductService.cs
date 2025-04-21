using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderItemService _orderItemService;
    private readonly ICategoryService _categoryService;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductService(
        IProductRepository productRepository, 
        ICategoryService categoryService, 
        IOrderItemService orderItemService, 
        ILoggingService logger, 
        ICacheService cacheService, 
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
        _orderItemService = orderItemService;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task ProductCacheInvalidateAsync()
    {   
        try
        {
            await _cacheService.RemoveAsync(AllProductsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while invalidating cache: {Message}", ex.Message);
            throw;
        }
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

            List<ProductResponseDto> productResponseDtos = products.Select(p => new ProductResponseDto
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
            {
                return cachedProduct;
            }

            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == requestId) ?? throw new Exception("Product not found");
            
            ProductResponseDto productResponse = new ProductResponseDto
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

    public async Task CreateProductAsync(ProductCreateRequestDto productCreateRequest)
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

            product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

            await _productRepository.Create(product);
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
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
            product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

            _productRepository.Update(product);
            await _orderItemService.ClearOrderItemsIncludeProductAsync(product);
            
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();

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

            _productRepository.Delete(product);
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Product deleted successfully: {Product}", product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UpdateProductStockAsync(List<Domain.Model.OrderItem> orderItems)
    {
        try
        {
            var products = await _productRepository.Read();
            foreach (var orderItem in orderItems)
            {
                var cartProduct = products.FirstOrDefault(p => p.ProductId == orderItem.ProductId);

                if (cartProduct == null)
                {
                    throw new Exception($"Product with ID {orderItem.ProductId} not found in the database.");
                }

                int Quantity = orderItem.Quantity;
                cartProduct.StockQuantity -= Quantity;
                _productRepository.Update(cartProduct);
            }

            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Product stock updated successfully: {Products}", products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product stock: {Message}", ex.Message);
            throw;
        }
    }
}
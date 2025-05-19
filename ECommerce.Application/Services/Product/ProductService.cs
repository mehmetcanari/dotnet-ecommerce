using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Services.Base;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Application.Services.Product;

public class ProductService : ServiceBase, IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly ICategoryService _categoryService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductService(
        IProductRepository productRepository, 
        ICategoryService categoryService, 
        IBasketItemService basketItemService, 
        ILoggingService logger, 
        ICacheService cacheService, 
        IUnitOfWork unitOfWork, 
        ICategoryRepository categoryRepository,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
        _basketItemService = basketItemService;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<ProductResponseDto>>> GetAllProductsAsync()
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

    public async Task<Result<ProductResponseDto>> GetProductWithIdAsync(int productId)
    {
        try
        {
            var expirationTime = TimeSpan.FromMinutes(60);
            var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(string.Format(ProductCacheKey, productId));
            if (cachedProduct != null)
            {
                return Result<ProductResponseDto>.Success(cachedProduct);
            }

            var product = await _productRepository.GetProductById(productId);
            if (product == null)
            {
                _logger.LogWarning("Product with id {Id} not found", productId);
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

            await _cacheService.SetAsync(string.Format(ProductCacheKey, productId), productResponse, expirationTime);
            return Result<ProductResponseDto>.Success(productResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product with id: {Message}", ex.Message);
            return Result<ProductResponseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> CreateProductAsync(ProductCreateRequestDto productCreateRequest)
    {
        try
        {
            var validationResult = await ValidateAsync(productCreateRequest);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);
            
            var existingProduct = await _productRepository.CheckProductExistsWithName(productCreateRequest.Name);
            if (existingProduct)
            {
                return Result.Failure("Product with this name already exists");
            }
            
            var category = await _categoryRepository.GetCategoryById(productCreateRequest.CategoryId);
            if (category == null)
            {
                return Result.Failure("Category not found");
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
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding product: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequest)
    {
        try
        {
            var validationResult = await ValidateAsync(productUpdateRequest);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);
            
            var category = await _categoryRepository.GetCategoryById(productUpdateRequest.CategoryId);
            var product = await _productRepository.GetProductById(id);
            
            if (category == null)
            {
                return Result.Failure("Category not found");
            }
            
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

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
            await _basketItemService.ClearBasketItemsIncludeOrderedProductAsync(product);
            
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();

            _logger.LogInformation("Product updated successfully: {Product}", product);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

            _productRepository.Delete(product);
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Product deleted successfully: {Product}", product);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateProductStockAsync(List<Domain.Model.BasketItem> basketItems)
    {
        try
        {
            foreach (var basketItem in basketItems)
            {
                var cartProduct = await _productRepository.GetProductById(basketItem.ProductId);

                if (cartProduct == null)
                {
                    return Result.Failure($"Product with ID {basketItem.ProductId} not found in the database.");
                }

                var quantity = basketItem.Quantity;
                cartProduct.StockQuantity -= quantity;
                _productRepository.Update(cartProduct);
            }

            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Product stock updated successfully: {Products}", basketItems);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product stock: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task ProductCacheInvalidateAsync()
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
}
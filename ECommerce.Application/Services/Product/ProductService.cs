using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Events;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Product;

public class ProductService : BaseValidator, IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryService _categoryService;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public ProductService(
        IProductRepository productRepository, 
        ICategoryService categoryService, 
        ILoggingService logger, 
        ICacheService cacheService, 
        IUnitOfWork unitOfWork, 
        IServiceProvider serviceProvider,
        IMediator mediator) : base(serviceProvider)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result> CreateProductAsync(ProductCreateRequestDto productCreateRequest)
    {
        try
        {
            var validationResult = await ValidateAsync(productCreateRequest);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);
            
            var result = await _mediator.Send(new CreateProductCommand { ProductCreateRequest = productCreateRequest });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Product creation failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await SetProductCacheAsync();
            await _unitOfWork.Commit();
            
            _logger.LogInformation("Product created successfully: {ProductName}", productCreateRequest.Name);
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
            
            var result = await _mediator.Send(new UpdateProductCommand { Id = id, ProductUpdateRequest = productUpdateRequest });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Product update failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }
            
            await ProductCacheInvalidateAsync();
            await _categoryService.CategoryCacheInvalidateAsync();
            await SetProductCacheAsync();
            await _unitOfWork.Commit();

            _logger.LogInformation("Product updated successfully: {ProductName}", productUpdateRequest.Name);
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

            await _productRepository.Delete(product);
            
            var domainEvent = new ProductDeletedEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name
            };

            await _mediator.Publish(domainEvent);
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
            var result = await _mediator.Send(new UpdateProductStockCommand { BasketItems = basketItems });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Product stock update failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await _unitOfWork.Commit();
            await ProductCacheInvalidateAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating product");
            return Result.Failure("An unexpected error occurred while updating product");
        }
    }

    private async Task ProductCacheInvalidateAsync()
    {   
        try
        {
            await _cacheService.RemoveAsync(CacheKeys.AllProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while invalidating cache: {Message}", ex.Message);
            throw;
        }
    }

    private async Task SetProductCacheAsync()
    {
        try
        {
            var products = await _productRepository.Read();
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

            await _cacheService.SetAsync(CacheKeys.AllProducts, productResponses, TimeSpan.FromMinutes(60));
            _logger.LogInformation("Successfully cached {Count} products", productResponses.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting product cache: {Message}", ex.Message);
            throw;   
        }
    }
}


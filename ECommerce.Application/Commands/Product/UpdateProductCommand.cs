using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductCommand : IRequest<Result>
{
    public required int Id { get; set; }
    public required ProductUpdateRequestDto ProductUpdateRequest { get; set; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly ILoggingService _logger;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, IBasketItemService basketItemService, ILoggingService logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _basketItemService = basketItemService;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetCategoryById(request.ProductUpdateRequest.CategoryId);
            var product = await _productRepository.GetProductById(request.Id);
            
            if (category == null)
            {
                return Result.Failure("Category not found");
            }
            
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

            product.Name = request.ProductUpdateRequest.Name;
            product.Description = request.ProductUpdateRequest.Description;
            product.Price = request.ProductUpdateRequest.Price;
            product.DiscountRate = request.ProductUpdateRequest.DiscountRate;
            product.ImageUrl = request.ProductUpdateRequest.ImageUrl;
            product.StockQuantity = request.ProductUpdateRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;
            product.CategoryId = category.CategoryId;
            product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

            await _productRepository.UpdateAsync(product);
            await _basketItemService.ClearBasketItemsIncludeOrderedProductAsync(product);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {Id}", request.Id);
            return Result.Failure("An error occurred while updating the product");
        }
    }
}
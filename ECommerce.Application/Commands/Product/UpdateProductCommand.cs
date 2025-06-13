using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
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

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBasketItemService basketItemService,
        ILoggingService logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _basketItemService = basketItemService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateRequest(request.Id, request.ProductUpdateRequest);
            if (!validationResult.IsSuccess)
            {
                return Result.Failure(validationResult.Error);
            }

            var (product, category) = validationResult.Data;
            UpdateProduct(product, category, request.ProductUpdateRequest);
            await _basketItemService.ClearBasketItemsIncludeOrderedProductAsync(product);

            _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {Name}", 
                product.ProductId, product.Name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {Id}: {Message}", request.Id, ex.Message);
            return Result.Failure("An error occurred while updating the product");
        }
    }

    private async Task<Result<(Domain.Model.Product Product, Domain.Model.Category Category)>> ValidateRequest(
        int productId,
        ProductUpdateRequestDto request)
    {
        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Product update failed. Category with ID {CategoryId} not found", request.CategoryId);
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure("Category not found");
        }

        var product = await _productRepository.GetProductById(productId);
        if (product == null)
        {
            _logger.LogWarning("Product update failed. Product with ID {ProductId} not found", productId);
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure("Product not found");
        }

        return Result<(Domain.Model.Product, Domain.Model.Category)>.Success((product, category));
    }

    private void UpdateProduct(
        Domain.Model.Product product,
        Domain.Model.Category category,
        ProductUpdateRequestDto request)
    {
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountRate = request.DiscountRate;
        product.ImageUrl = request.ImageUrl;
        product.StockQuantity = request.StockQuantity;
        product.ProductUpdated = DateTime.UtcNow;
        product.CategoryId = category.CategoryId;
        product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

        _productRepository.Update(product);
    }
}
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductCommand : IRequest<Result>
{
    public required ProductUpdateRequestDto Model { get; set; }
    public required Guid Id { get; set; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly ILoggingService _logger;
    private readonly IMessageBroker _messageBroker;
    private readonly IMediator _mediator;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, IBasketItemService basketItemService, ILoggingService logger,
        IMessageBroker messageBroker, IMediator mediator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _basketItemService = basketItemService;
        _logger = logger;
        _messageBroker = messageBroker;
        _mediator = mediator;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateRequest(request.Id, request.Model);
            if (validationResult.IsFailure && validationResult.Message is not null)
            {
                return Result.Failure(validationResult.Message);
            }

            var (product, category) = validationResult.Data;
            var updatedProduct = UpdateProduct(product, category, request.Model);
            await _basketItemService.ClearBasketItemsIncludeOrderedProductAsync(product);

            var domainEvent = new ProductUpdatedEvent
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                DiscountRate = updatedProduct.DiscountRate,
                ImageUrl = updatedProduct.ImageUrl ?? string.Empty,
                StockQuantity = updatedProduct.StockQuantity,
                CategoryId = updatedProduct.CategoryId,
                ProductCreated = updatedProduct.CreatedOn,
                ProductUpdated = updatedProduct.UpdatedOn
            };

            await _mediator.Publish(domainEvent, cancellationToken);
            await _messageBroker.PublishAsync(domainEvent, "product_exchange", "product.updated");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorUpdatingProduct, request.Id, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProduct);
        }
    }

    private async Task<Result<(Domain.Model.Product Product, Domain.Model.Category Category)>> ValidateRequest(Guid productId, ProductUpdateRequestDto request)
    {
        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure(ErrorMessages.CategoryNotFound);
        }

        var product = await _productRepository.GetProductById(productId);
        if (product == null)
        {
            _logger.LogWarning(ErrorMessages.ProductNotFound, productId);
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure(ErrorMessages.ProductNotFound);
        }

        return Result<(Domain.Model.Product, Domain.Model.Category)>.Success((product, category));
    }

    private Domain.Model.Product UpdateProduct(Domain.Model.Product product, Domain.Model.Category category, ProductUpdateRequestDto request)
    {
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountRate = request.DiscountRate;
        product.ImageUrl = request.ImageUrl;
        product.StockQuantity = request.StockQuantity;
        product.UpdatedOn = DateTime.UtcNow;
        product.CategoryId = category.Id;
        product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

        _productRepository.Update(product);
        return product;
    }
}
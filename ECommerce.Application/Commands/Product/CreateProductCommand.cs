using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Commands.Product;

public class CreateProductCommand(ProductCreateRequestDto request) : IRequest<Result>
{
    public readonly ProductCreateRequestDto Model = request;
}

public class CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, ILoggingService logger, IMessageBroker messageBroker,
    IMediator mediator, IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result>
{
    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var productValidationResult = await ValidateProductCreateRequest(request.Model);
            if (productValidationResult is { IsFailure: true, Message: not null })
                return Result.Failure(productValidationResult.Message);

            var categoryValidationResult = await ValidateCategory(request.Model.CategoryId);
            if (categoryValidationResult is { IsFailure: true, Message: not null })
                return Result.Failure(categoryValidationResult.Message);

            var product = CreateProductEntity(request.Model);
            await productRepository.Create(product, cancellationToken);
            
            await unitOfWork.Commit();

            var domainEvent = new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl ?? string.Empty,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ProductCreated = product.CreatedOn,
                ProductUpdated = product.UpdatedOn
            };

            await mediator.Publish(domainEvent, cancellationToken);
            await messageBroker.PublishAsync(domainEvent, "product_exchange", "product.created");
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingProduct, ex.Message);
            return Result.Failure(ErrorMessages.ErrorCreatingProduct);
        }
    }

    private async Task<Result> ValidateProductCreateRequest(ProductCreateRequestDto request)
    {
        var existingProduct = await productRepository.CheckExistsWithName(request.Name);
        return existingProduct ? Result.Failure(ErrorMessages.ProductExists) : Result.Success();
    }

    private async Task<Result<Domain.Model.Category>> ValidateCategory(Guid categoryId)
    {
        var category = await categoryRepository.GetById(categoryId);
        if (category is null)
            return Result<Domain.Model.Category>.Failure(ErrorMessages.CategoryNotFound);
        
        return Result<Domain.Model.Category>.Success(category);
    }


    private static Domain.Model.Product CreateProductEntity(ProductCreateRequestDto request)
    {
        var product = new Domain.Model.Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountRate = request.DiscountRate,
            ImageUrl = request.ImageUrl,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };

        product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);
        return product;
    }
}
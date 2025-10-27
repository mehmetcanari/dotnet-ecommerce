using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductCommand(ProductUpdateRequestDto request) : IRequest<Result>
{
    public readonly ProductUpdateRequestDto Model = request;
}

public class UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogService logger, IMessageBroker messageBroker, IMediator mediator, IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateRequest(request.Model.Id, request.Model);
            if (validationResult is { IsFailure: true, Message: not null })
            {
                return Result.Failure(validationResult.Message);
            }

            var (product, category) = validationResult.Data;
            var updatedProduct = UpdateProduct(product, category, request.Model);
            await unitOfWork.Commit();

            var productUpdatedEvent = new ProductUpdatedEvent
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                DiscountRate = updatedProduct.DiscountRate,
                ImageUrl = updatedProduct.ImageUrl ?? string.Empty,
                StockQuantity = updatedProduct.StockQuantity,
                CategoryId = updatedProduct.CategoryId,
            };

            await mediator.Publish(productUpdatedEvent, cancellationToken); //neden mediator ile elastice ulasiyoruz?
            await messageBroker.Publish(productUpdatedEvent, "product_exchange", "product.updated");

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingProduct, request.Model.Id, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProduct);
        }
    }

    private async Task<Result<(Domain.Model.Product Product, Domain.Model.Category Category)>> ValidateRequest(Guid productId, ProductUpdateRequestDto request)
    {
        var category = await categoryRepository.GetById(request.CategoryId);
        if (category == null)
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure(ErrorMessages.CategoryNotFound);

        var product = await productRepository.GetById(productId);
        if (product == null)
            return Result<(Domain.Model.Product, Domain.Model.Category)>.Failure(ErrorMessages.ProductNotFound);

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

        productRepository.Update(product);
        return product;
    }
}
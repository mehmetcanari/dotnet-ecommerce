using ECommerce.Application.Abstract;
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

public class CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogService logger, IMessageBroker messageBroker,
    IMediator mediator, IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result>
{
    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingProduct = await productRepository.CheckExistsWithName(request.Model.Name, cancellationToken);
            if(existingProduct)
                return Result.Failure(ErrorMessages.ProductExists);

            var category = await categoryRepository.GetById(request.Model.CategoryId, cancellationToken);
            if(category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            var product = new Domain.Model.Product
            {
                Name = request.Model.Name,
                Description = request.Model.Description,
                Price = request.Model.Price,
                DiscountRate = request.Model.DiscountRate,
                ImageUrl = request.Model.ImageUrl,
                StockQuantity = request.Model.StockQuantity,
                CategoryId = request.Model.CategoryId
            };

            product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);
            await productRepository.Create(product, cancellationToken);
            
            await unitOfWork.Commit();

            var productCreatedEvent = new ProductCreatedEvent
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

            await mediator.Publish(productCreatedEvent, cancellationToken);
            await messageBroker.Publish(productCreatedEvent, "product_exchange", "product.created");
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingProduct, ex.Message);
            return Result.Failure(ErrorMessages.ErrorCreatingProduct);
        }
    }
}
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
            var category = await categoryRepository.GetById(request.Model.CategoryId, cancellationToken);
            if (category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            var product = await productRepository.GetById(request.Model.Id, cancellationToken);
            if (product is null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            product.Name = request.Model.Name;
            product.Description = request.Model.Description;
            product.Price = MathService.CalculateDiscount(request.Model.Price, request.Model.DiscountRate);
            product.DiscountRate = request.Model.DiscountRate;
            product.ImageUrl = request.Model.ImageUrl;
            product.StockQuantity = request.Model.StockQuantity;
            product.UpdatedOn = DateTime.UtcNow;

            await productRepository.Update(product, cancellationToken);
            await unitOfWork.Commit();

            var productUpdatedEvent = new ProductUpdatedEvent
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl ?? string.Empty,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
            };

            await mediator.Publish(productUpdatedEvent, cancellationToken); //mediator ile ulasimi kapat
            await messageBroker.Publish(productUpdatedEvent, "product_exchange", "product.updated");//rabbitmq gerekmedigi icin gozden gecirilecek

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingProduct, request.Model.Id, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProduct);
        }
    }
}
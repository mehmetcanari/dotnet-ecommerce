using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Events;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class UpdateProductStockCommand : IRequest<Result>
{
    public required List<BasketItem> Model { get; set; }
}

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediator _mediator;
    private readonly ILoggingService _logger;

    public UpdateProductStockCommandHandler(IProductRepository productRepository, IMediator mediator, ILoggingService logger)
    {
        _productRepository = productRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProducts = new List<Domain.Model.Product>();
            
            foreach (var item in request.Model)
            {
                var validationResult = await ValidateAndUpdateStock(item);
                if (!validationResult.result.IsSuccess)
                {
                    return validationResult.result;
                }
                updatedProducts.Add(validationResult.product);
            }

            foreach (var product in updatedProducts)
            {
                var domainEvent = new ProductStockUpdatedEvent
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

                await _mediator.Publish(domainEvent, cancellationToken);
            }

            _logger.LogInformation(ErrorMessages.UpdateProductStockSuccess, request.Model.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorUpdatingProductStock, ex.Message);
            return Result.Failure(ErrorMessages.ErrorUpdatingProductStock);
        }
    }

    private async Task<(Result result, Domain.Model.Product product)> ValidateAndUpdateStock(BasketItem item)
    {
        var product = await _productRepository.GetById(item.ProductId);
        if (product == null)
            return (Result.Failure(ErrorMessages.ProductNotFound), null!);
        
        if (product.StockQuantity < item.Quantity)
            return (Result.Failure(ErrorMessages.StockNotAvailable), null!);
        

        UpdateProductStock(product, item.Quantity);
        return (Result.Success(), product);
    }

    private void UpdateProductStock(Domain.Model.Product product, int quantity)
    {
        product.StockQuantity -= quantity;
        product.UpdatedOn = DateTime.UtcNow;
        _productRepository.UpdateStock(product.Id, product.StockQuantity);

        _logger.LogInformation(ErrorMessages.UpdateProductStockSuccess, product.Id, product.Name, product.StockQuantity);
    }
}
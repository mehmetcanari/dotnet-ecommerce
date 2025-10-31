using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class CreateProductCommand(ProductCreateRequestDto request) : IRequest<Result>
{
    public readonly ProductCreateRequestDto Model = request;
}

public class CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork, IElasticSearchService elasticSearchService) : IRequestHandler<CreateProductCommand, Result>
{
    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingProduct = await productRepository.CheckExistsWithName(request.Model.Name, cancellationToken);
            if (existingProduct)
                return Result.Failure(ErrorMessages.ProductExists);

            var category = await categoryRepository.GetById(request.Model.CategoryId, cancellationToken);
            if (category is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            var product = new Domain.Model.Product
            {
                Name = request.Model.Name,
                Description = request.Model.Description,
                Price = MathService.CalculateDiscount(request.Model.Price, request.Model.DiscountRate),
                DiscountRate = request.Model.DiscountRate,
                ImageUrl = request.Model.ImageUrl,
                StockQuantity = request.Model.StockQuantity,
                CategoryId = request.Model.CategoryId
            };

            await elasticSearchService.IndexAsync(product, "products", product.Id.ToString(), cancellationToken);
            await productRepository.Create(product, cancellationToken);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingProduct, ex.Message);
            return Result.Failure(ErrorMessages.ErrorCreatingProduct);
        }
    }
}
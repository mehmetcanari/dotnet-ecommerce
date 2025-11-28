using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetProductByIdQuery(Guid id) : IRequest<Result<ProductResponseDto>>
{
    public readonly Guid Id = id;
}

public class GetProductByIdQueryHandler(IProductRepository productRepository, ILogService logger) : IRequestHandler<GetProductByIdQuery, Result<ProductResponseDto>>
{
    public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productRepository.GetById(request.Id, cancellationToken);
            if (product == null)
                return Result<ProductResponseDto>.Failure(ErrorMessages.ProductNotFound);

            var response = MapToResponseDto(product);

            return Result<ProductResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<ProductResponseDto>.Failure(ex.Message);
        }
    }

    private ProductResponseDto MapToResponseDto(Domain.Model.Product product) => new()
    {
        Id = product.Id,
        ProductName = product.Name,
        Description = product.Description,
        Price = product.Price,
        DiscountRate = product.DiscountRate,
        ImageUrl = product.ImageUrl,
        StockQuantity = product.StockQuantity,
        CategoryId = product.CategoryId
    };
}
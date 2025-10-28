using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetProductByIdQuery(Guid id) : IRequest<Result<ProductResponseDto>>
{
    public readonly Guid Id = id;
}

public class GetProductByIdQueryHandler(IProductRepository productRepository, ICacheService cacheService, ILogService logger) : IRequestHandler<GetProductByIdQuery, Result<ProductResponseDto>>
{
    private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(60);

    public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedProduct = await cacheService.GetAsync<ProductResponseDto>(string.Format(CacheKeys.ProductById, request.Id));
            if (cachedProduct != null)
                return Result<ProductResponseDto>.Success(cachedProduct);

            var product = await productRepository.GetById(request.Id, cancellationToken);
            if (product == null)
                return Result<ProductResponseDto>.Failure(ErrorMessages.ProductNotFound);
            
            var productResponse = MapToResponseDto(product);
            await cacheService.SetAsync(string.Format(CacheKeys.ProductById, product.Id), productResponse, ExpirationTime);

            return Result<ProductResponseDto>.Success(productResponse);
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
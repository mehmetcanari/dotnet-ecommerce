using ECommerce.Application.DTO.Response.Product;
using MediatR;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Application.Abstract;

namespace ECommerce.Application.Queries.Product;

public class GetAllProductsQuery : IRequest<Result<List<ProductResponseDto>>>{}

public class GetAllProductsQueryHandler(IProductRepository productRepository, ICacheService cacheService, ILogService logger) : IRequestHandler<GetAllProductsQuery, Result<List<ProductResponseDto>>>
{
    private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(60);

    public async Task<Result<List<ProductResponseDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedProducts = await cacheService.GetAsync<List<ProductResponseDto>>(CacheKeys.AllProducts);

            if (cachedProducts is { Count: > 0 })
                return Result<List<ProductResponseDto>>.Success(cachedProducts);

            var products = await productRepository.Read(cancellationToken: cancellationToken);

            if (products.Count == 0)
                throw new Exception(ErrorMessages.ProductNotFound);

            var productResponses = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId
            }).ToList();

            await cacheService.SetAsync(CacheKeys.AllProducts, productResponses, ExpirationTime);
            
            return Result<List<ProductResponseDto>>.Success(productResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ex.Message);
        }
    }
}
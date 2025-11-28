using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.Product;
using ECommerce.Shared.Enum;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Product;

public class GetAllProductsQuery(QueryPagination pagination) : IRequest<Result<List<ProductResponseDto>>>
{
    public readonly QueryPagination Pagination = pagination;
}

public class GetAllProductsQueryHandler(IProductRepository productRepository, ICacheService cache, ILogService logger) : IRequestHandler<GetAllProductsQuery, Result<List<ProductResponseDto>>>
{
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

    public async Task<Result<List<ProductResponseDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedProducts = await cache.GetAsync<List<ProductResponseDto>>(CacheKeys.Products, cancellationToken);
            if (cachedProducts is { Count: > 0 })
                return Result<List<ProductResponseDto>>.Success(cachedProducts);

            var products = await productRepository.Read(request.Pagination.Page, request.Pagination.PageSize, cancellationToken);
            if (products.Count == 0)
                return Result<List<ProductResponseDto>>.Failure(ErrorMessages.ProductNotFound);

            var response = products.Select(p => new ProductResponseDto
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

            await cache.SetAsync(CacheKeys.Products, response, CacheExpirationType.Sliding, _expiration, cancellationToken);

            return Result<List<ProductResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<ProductResponseDto>>.Failure(ex.Message);
        }
    }
}
using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.Wishlist;
using ECommerce.Shared.Enum; 
using ECommerce.Shared.Wrappers; 
using MediatR;

namespace ECommerce.Application.Queries.Wishlist;

public class GetWishlistItemQuery(QueryPagination pagination) : IRequest<Result<List<WishlistItemResponseDto>>>
{
    public readonly QueryPagination Pagination = pagination;
}

public class GetWishlistItemQueryHandler(IWishlistRepository wishlistRepository, ILogService logger, ICacheService cacheService, ICurrentUserService currentUserService)
    : IRequestHandler<GetWishlistItemQuery, Result<List<WishlistItemResponseDto>>>
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(30);

    public async Task<Result<List<WishlistItemResponseDto>>> Handle(GetWishlistItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            string cacheKey = $"{CacheKeys.Wishlist}:{userId}";

            var cachedWishlistItems = await cacheService.GetAsync<List<WishlistItemResponseDto>>(cacheKey, cancellationToken);

            if (cachedWishlistItems is { Count: > 0 })
                return Result<List<WishlistItemResponseDto>>.Success(cachedWishlistItems);

            var wishlistItems = await wishlistRepository.Read(Guid.Parse(userId), request.Pagination.Page, request.Pagination.PageSize, cancellationToken);
            if (wishlistItems.Count == 0)
                return Result<List<WishlistItemResponseDto>>.Failure(ErrorMessages.WishlistItemNotFound);

            var response = wishlistItems.Select(w => new WishlistItemResponseDto
            {
                UserId = w.UserId,
                ProductId = w.ProductId
            }).ToList();

            await cacheService.SetAsync(cacheKey, response, CacheExpirationType.Sliding, _ttl, cancellationToken);

            return Result<List<WishlistItemResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<WishlistItemResponseDto>>.Failure(ex.Message);
        }
    }
}
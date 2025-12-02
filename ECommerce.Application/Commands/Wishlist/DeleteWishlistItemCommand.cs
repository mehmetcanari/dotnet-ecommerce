using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Request.Wishlist;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Wishlist;

public class DeleteWishlistItemCommand(WishlistItemDeleteRequestDto request) : IRequest<Result>
{
    public readonly WishlistItemDeleteRequestDto Model = request;
}

public class DeleteWishlistItemCommandHandler(IWishlistRepository wishlistRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService) : IRequestHandler<DeleteWishlistItemCommand, Result>
{
    public async Task<Result> Handle(DeleteWishlistItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = Guid.Parse(currentUserService.GetUserId());

            var wishlistItem = await wishlistRepository.GetById(userId, request.Model.ProductId, cancellationToken);
            if (wishlistItem is null)
                return Result.Failure(ErrorMessages.WishlistItemNotFound);
            
            await cacheService.RemoveAsync(CacheKeys.Wishlist, cancellationToken);
            await wishlistRepository.Delete(wishlistItem, cancellationToken);
            
            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorDeletingWishlistItem, ex.Message);
            return Result.Failure(ErrorMessages.ErrorDeletingWishlistItem);
        }
    }
}
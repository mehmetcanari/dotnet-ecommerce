using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Request.Wishlist;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Wishlist;

public class CreateWishlistItemCommand(WishlistItemCreateRequestDto request) : IRequest<Result>
{
    public readonly WishlistItemCreateRequestDto Model = request;
}

public class CreateWishlistItemCommandHandler(IWishlistRepository wishlistRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService) : IRequestHandler<CreateWishlistItemCommand, Result>
{
    public async Task<Result> Handle(CreateWishlistItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = Guid.Parse(currentUserService.GetUserId());

            var exists = await wishlistRepository.Exists(userId, request.Model.ProductId, cancellationToken);
            if (exists)
                return Result.Failure(ErrorMessages.WishlistItemAlreadyExists);
            
            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = request.Model.ProductId,
            };

            await cacheService.RemoveAsync(CacheKeys.Wishlist, cancellationToken);
            await wishlistRepository.Create(wishlistItem, cancellationToken);
            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingWishlistItem, ex.Message);
            return Result.Failure(ErrorMessages.ErrorCreatingWishlistItem);
        }
    }
}
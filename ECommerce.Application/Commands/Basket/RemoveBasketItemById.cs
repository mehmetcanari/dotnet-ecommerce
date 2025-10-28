using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class RemoveBasketItemById(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class RemoveBasketItemByIdHandler(ICurrentUserService currentUserService, IBasketItemRepository basketItemRepository, IUnitOfWork unitOfWork, ILogService logService) : IRequestHandler<RemoveBasketItemById, Result>
{
    public async Task<Result> Handle(RemoveBasketItemById request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountNotAuthorized);

            var basketItem = await basketItemRepository.GetById(request.Id, cancellationToken);
            if (basketItem is null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            if(basketItem.UserId.ToString() != userId)
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            basketItemRepository.Delete(basketItem);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}

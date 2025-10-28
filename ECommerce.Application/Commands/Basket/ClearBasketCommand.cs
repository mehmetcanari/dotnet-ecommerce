using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class ClearBasketCommand : IRequest<Result> { }

public class ClearBasketCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<ClearBasketCommand, Result>
{
    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var basketItems = await basketItemRepository.GetActiveItems(Guid.Parse(userId), cancellationToken);
            if (basketItems.Count == 0)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            foreach (var basketItem in basketItems)
            {
                basketItemRepository.Delete(basketItem);
            }

            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}
using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class ClearBasketCommand : IRequest<Result> { }

public class ClearBasketCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, IAccountRepository accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<ClearBasketCommand, Result>
{
    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var user = await accountRepository.GetByEmail(email, cancellationToken);
            if (user is null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItems = await basketItemRepository.GetActiveItems(user, cancellationToken);
            if (basketItems.Count == 0)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            DeleteItems(basketItems);

            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private void DeleteItems(List<Domain.Model.BasketItem> basketItems)
    {
        foreach (var basketItem in basketItems)
        {
            basketItemRepository.Delete(basketItem);
        }
    }
}
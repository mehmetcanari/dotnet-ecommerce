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

public class RemoveBasketItemByIdHandler(ICurrentUserService currentUserService, IAccountRepository accountRepository, IBasketItemRepository basketItemRepository, IUnitOfWork unitOfWork, ILogService logService) : IRequestHandler<RemoveBasketItemById, Result>
{
    public async Task<Result> Handle(RemoveBasketItemById request, CancellationToken cancellationToken)
    {
        try
        {
            var email = currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var user = await accountRepository.GetByEmail(email, cancellationToken);
            if (user is null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItem = await basketItemRepository.GetById(request.Id, cancellationToken);
            if (basketItem is null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            if(basketItem.UserId != user.Id)
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

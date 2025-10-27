using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class DeleteAllUnorderedBasketItemsCommand : IRequest<Result> { }

public class DeleteAllUnorderedBasketItemsCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, IAccountRepository accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteAllUnorderedBasketItemsCommand, Result>
{
    public async Task<Result> Handle(DeleteAllUnorderedBasketItemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = GetValidatedUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var accountResult = await ValidateAndGetAccount(email);
            if (accountResult.IsFailure || accountResult.Data == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItemsResult = await GetAndValidateUnorderedBasketItems(accountResult.Data);
            if (basketItemsResult.IsFailure || basketItemsResult.Data == null || basketItemsResult.Data.Count == 0)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            DeleteBasketItems(basketItemsResult.Data);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        return email;
    }

    private async Task<Result<Domain.Model.User>> ValidateAndGetAccount(string email)
    {
        var account = await accountRepository.GetByEmail(email);
        if (account == null)
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetAndValidateUnorderedBasketItems(Domain.Model.User account)
    {
        var basketItems = await basketItemRepository.GetUnorderedItems(account);
        if (!basketItems.Any())
            return Result<List<Domain.Model.BasketItem>>.Failure(ErrorMessages.BasketItemNotFound);

        return Result<List<Domain.Model.BasketItem>>.Success(basketItems);
    }

    private void DeleteBasketItems(List<Domain.Model.BasketItem> basketItems)
    {
        foreach (var basketItem in basketItems)
        {
            basketItemRepository.Delete(basketItem);
        }
    }
}
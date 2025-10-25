using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class DeleteAllNonOrderedBasketItemsCommand : IRequest<Result> { }

public class DeleteAllNonOrderedBasketItemsCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILoggingService logger, IAccountRepository accountRepository) : IRequestHandler<DeleteAllNonOrderedBasketItemsCommand, Result>
{
    public async Task<Result> Handle(DeleteAllNonOrderedBasketItemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();
            if (string.IsNullOrEmpty(emailResult))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var accountResult = await ValidateAndGetAccount(emailResult);
            if (accountResult.IsFailure || accountResult.Data == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItemsResult = await GetAndValidateNonOrderedBasketItems(accountResult.Data);
            if (basketItemsResult.IsFailure || basketItemsResult.Data == null || !basketItemsResult.Data.Any())
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            DeleteBasketItems(basketItemsResult.Data);

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

    private async Task<Result<List<Domain.Model.BasketItem>>> GetAndValidateNonOrderedBasketItems(Domain.Model.User account)
    {
        var basketItems = await basketItemRepository.GetNonOrdereds(account);
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
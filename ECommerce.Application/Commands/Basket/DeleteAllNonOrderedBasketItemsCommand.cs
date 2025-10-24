using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class DeleteAllNonOrderedBasketItemsCommand : IRequest<Result> { }

public class DeleteAllNonOrderedBasketItemsCommandHandler : IRequestHandler<DeleteAllNonOrderedBasketItemsCommand, Result>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public DeleteAllNonOrderedBasketItemsCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILoggingService logger, IAccountRepository accountRepository)
    {
        _basketItemRepository = basketItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public async Task<Result> Handle(DeleteAllNonOrderedBasketItemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();
            if (emailResult == null)
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var accountResult = await ValidateAndGetAccount(emailResult);
            if (accountResult.IsFailure)
                return Result.Failure(ErrorMessages.AccountNotFound);

            if(accountResult.Data == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItemsResult = await GetAndValidateNonOrderedBasketItems(accountResult.Data);
            if (basketItemsResult.IsFailure)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            if(basketItemsResult.Data == null || !basketItemsResult.Data.Any())
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            DeleteBasketItems(basketItemsResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        return email;
    }

    private async Task<Result<Domain.Model.User>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetByEmail(email);
        if (account == null)
        {
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);
        }

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetAndValidateNonOrderedBasketItems(Domain.Model.User account)
    {
        var basketItems = await _basketItemRepository.GetNonOrdereds(account);
        if (!basketItems.Any())
        {
            return Result<List<Domain.Model.BasketItem>>.Failure(ErrorMessages.BasketItemNotFound);
        }

        return Result<List<Domain.Model.BasketItem>>.Success(basketItems);
    }

    private void DeleteBasketItems(List<Domain.Model.BasketItem> basketItems)
    {
        foreach (var basketItem in basketItems)
        {
            _basketItemRepository.Delete(basketItem);
        }
    }
}
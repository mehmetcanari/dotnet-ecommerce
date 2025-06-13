using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class DeleteAllNonOrderedBasketItemsCommand : IRequest<Result> { }

public class DeleteAllNonOrderedBasketItemsCommandHandler : IRequestHandler<DeleteAllNonOrderedBasketItemsCommand, Result>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public DeleteAllNonOrderedBasketItemsCommandHandler(
        IBasketItemRepository basketItemRepository,
        ICurrentUserService currentUserService,
        ILoggingService logger,
        IAccountRepository accountRepository)
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
            var emailResult = await GetValidatedUserEmail();
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var accountResult = await ValidateAndGetAccount(emailResult.Data);
            if (accountResult.IsFailure)
                return Result.Failure(accountResult.Error);

            var basketItemsResult = await GetAndValidateNonOrderedBasketItems(accountResult.Data);
            if (basketItemsResult.IsFailure)
                return Result.Failure(basketItemsResult.Error);

            DeleteBasketItems(basketItemsResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting non-ordered basket items");
            return Result.Failure("An error occurred while processing your request");
        }
    }

    private async Task<Result<string>> GetValidatedUserEmail()
    {
        var emailResult = await Task.FromResult(_currentUserService.GetCurrentUserEmail());
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<string>.Failure(emailResult.Error);
        }

        if (string.IsNullOrEmpty(emailResult.Data))
        {
            _logger.LogWarning("User email is null or empty");
            return Result<string>.Failure("Email is not available");
        }

        return Result<string>.Success(emailResult.Data);
    }

    private async Task<Result<Domain.Model.Account>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            _logger.LogWarning("Account not found for email: {Email}", email);
            return Result<Domain.Model.Account>.Failure("Account not found");
        }

        return Result<Domain.Model.Account>.Success(account);
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetAndValidateNonOrderedBasketItems(Domain.Model.Account account)
    {
        var basketItems = await _basketItemRepository.GetNonOrderedBasketItems(account);
        if (!basketItems.Any())
        {
            _logger.LogInformation("No basket items found to delete for account: {AccountId}", account.Id);
            return Result<List<Domain.Model.BasketItem>>.Failure("No basket items found to delete");
        }

        return Result<List<Domain.Model.BasketItem>>.Success(basketItems);
    }

    private void DeleteBasketItems(List<Domain.Model.BasketItem> basketItems)
    {
        foreach (var basketItem in basketItems)
        {
            _basketItemRepository.Delete(basketItem);
        }

        _logger.LogInformation("Successfully deleted {Count} basket items", basketItems.Count);
    }
}
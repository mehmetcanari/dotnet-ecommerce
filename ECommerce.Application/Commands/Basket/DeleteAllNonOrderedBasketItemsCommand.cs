using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class DeleteAllNonOrderedBasketItemsCommand : IRequest<Result>{}

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
           var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result.Failure(emailResult.Error);
            }
            
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null");
                return Result.Failure("Email is not available");
            }
            
            var tokenAccount = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (tokenAccount == null)
                return Result.Failure("Account not found");
            
            var nonOrderedBasketItems = await _basketItemRepository.GetNonOrderedBasketItems(tokenAccount);

            if (nonOrderedBasketItems.Count == 0)
                return Result.Failure("No basket items found to delete");

            foreach (var basketItem in nonOrderedBasketItems)
            {
                _basketItemRepository.Delete(basketItem);
            }

            _logger.LogInformation("All basket items deleted successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting non-ordered basket items");
            return Result.Failure("An error occurred while processing your request");
        }
    }
}
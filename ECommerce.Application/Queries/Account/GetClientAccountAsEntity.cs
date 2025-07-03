using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using MediatR;

public class GetClientAccountAsEntityQuery : IRequest<Result<Account>>{}

public class GetClientAccountAsEntityQueryHandler : IRequestHandler<GetClientAccountAsEntityQuery, Result<Account>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetClientAccountAsEntityQueryHandler(IAccountRepository accountRepository, ILoggingService logger, ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Account>> Handle(GetClientAccountAsEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var validEmailResult = ValidateUser();
            if (!validEmailResult.IsSuccess)
            {
                return Result<Account>.Failure(validEmailResult.Error);
            }

            var account = await _accountRepository.GetAccountByEmail(validEmailResult.Data);
            if (account == null)
            {
                _logger.LogWarning("Account not found for email: {Email}", validEmailResult.Data);
                return Result<Account>.Failure($"User with email {validEmailResult.Data} not found");
            }

            return Result<Account>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            return Result<Account>.Failure("An unexpected error occurred");
        }
    }

    private Result<string> ValidateUser()
    {
        var emailResult = _currentUserService.GetCurrentUserEmail();
        if (emailResult is { IsSuccess: false, Error: not null })
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
}
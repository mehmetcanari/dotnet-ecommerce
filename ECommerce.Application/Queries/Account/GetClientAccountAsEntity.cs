using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
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
            if (validEmailResult.IsFailure && validEmailResult.Error is not null)
                return Result<Account>.Failure(validEmailResult.Error);
            
            if (string.IsNullOrEmpty(validEmailResult.Data))
                return Result<Account>.Failure(ErrorMessages.AccountEmailNotFound);
            
            var account = await _accountRepository.GetAccountByEmail(validEmailResult.Data);
            if (account == null)
                return Result<Account>.Failure(ErrorMessages.AccountEmailNotFound);

            return Result<Account>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountEmailNotFound, ex.Message);
            return Result<Account>.Failure(ErrorMessages.AccountEmailNotFound);
        }
    }

    private Result<string> ValidateUser()
    {
        var email = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Result<string>.Failure(ErrorMessages.AccountEmailNotFound);

        return Result<string>.Success(email);
    }
}
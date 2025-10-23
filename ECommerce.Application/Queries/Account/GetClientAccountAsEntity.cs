using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

public class GetClientAccountAsEntityQuery : IRequest<Result<User>>{}

public class GetClientAccountAsEntityQueryHandler : IRequestHandler<GetClientAccountAsEntityQuery, Result<User>>
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

    public async Task<Result<User>> Handle(GetClientAccountAsEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var validEmailResult = ValidateUser();
            if (validEmailResult.IsFailure && validEmailResult.Error is not null)
                return Result<User>.Failure(validEmailResult.Error);
            
            if (string.IsNullOrEmpty(validEmailResult.Data))
                return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);
            
            var account = await _accountRepository.GetAccountByEmail(validEmailResult.Data);
            if (account == null)
                return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);

            return Result<User>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountEmailNotFound, ex.Message);
            return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);
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
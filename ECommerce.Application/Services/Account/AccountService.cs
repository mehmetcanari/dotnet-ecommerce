using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Account;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Account;
public class AccountService : BaseValidator, IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;
    public AccountService(
        IAccountRepository accountRepository, 
        IUnitOfWork unitOfWork,
        ILoggingService logger, 
        IServiceProvider serviceProvider,
        IMediator mediator) : base(serviceProvider)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result> RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role)
    {
        try
        {
            var accountResult = await _mediator.Send(new RegisterUserCommand(createUserRequestDto, role));
            if (accountResult is { IsFailure: true, Message: not null })
                return Result.Failure(accountResult.Message);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountCreationFailed, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<Domain.Model.User>> GetAccountByEmailAsEntityAsync(string email)
    {
        try
        {
            var account = await _accountRepository.GetByEmail(email);
            if (account == null)
                return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);

            return Result<Domain.Model.User>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<Domain.Model.User>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    public async Task<Result> BanAccountAsync(AccountBanRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (validationResult is { IsSuccess: false, Message: not null })
                return Result.Failure(validationResult.Message);

            var result = await _mediator.Send(new BanAccountCommand(request));
            
            if (result is { IsFailure: true, Message: not null })
                return Result.Failure(result.Message);

            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UnbanAccountAsync(AccountUnbanRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (validationResult is { IsSuccess: false, Message: not null })
                return Result.Failure(validationResult.Message);

            var result = await _mediator.Send(new UnbanAccountCommand(request));

            if (result is { IsFailure: true, Message: not null })
                return Result.Failure(result.Message);

            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
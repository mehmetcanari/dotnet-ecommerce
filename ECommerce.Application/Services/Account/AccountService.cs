using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Account;

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
            var accountResult = await _mediator.Send(new CreateAccountCommand
            {
                AccountCreateRequest = createUserRequestDto,
                Role = role
            });

            if (accountResult is { IsFailure: true, Error: not null })
            {
                _logger.LogWarning("Account creation failed: {Error}", accountResult.Error);
                return Result.Failure(accountResult.Error);
            }

            var identityResult = await _mediator.Send(new CreateIdentityUserCommand
            {
                AccountRegisterRequestDto = createUserRequestDto,
                Role = role
            });

            if (identityResult is { IsFailure: true, Error: not null })
            {
                _logger.LogWarning("Identity user creation failed: {Error}", identityResult.Error);
                return Result.Failure(identityResult.Error);
            }

            var updateAccountGuidResult = await _mediator.Send(new UpdateAccountGuidCommand 
            { 
                Account = accountResult.Data!, 
                User = identityResult.Data!
            });
            
            if (updateAccountGuidResult is { IsFailure: true, Error: not null })
            {
                _logger.LogWarning("Account update failed: {Error}", updateAccountGuidResult.Error);
                return Result.Failure(updateAccountGuidResult.Error);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<Domain.Model.Account>> GetAccountByEmailAsEntityAsync(string email)
    {
        try
        {
            var account = await _accountRepository.GetAccountByEmail(email);
            if (account == null)
            {
                return Result<Domain.Model.Account>.Failure($"User with email {email} not found");
            }

            return Result<Domain.Model.Account>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            return Result<Domain.Model.Account>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result> BanAccountAsync(AccountBanRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (validationResult is { IsSuccess: false, Error: not null })
            {
                return Result.Failure(validationResult.Error);
            }

            var result = await _mediator.Send(new BanAccountCommand 
            {
                AccountBanRequestDto = request
            });
            
            if (result is { IsFailure: true, Error: not null })
            {
                _logger.LogWarning("Account ban failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while banning account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UnbanAccountAsync(AccountUnbanRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (validationResult is { IsSuccess: false, Error: not null })
            {
                return Result.Failure(validationResult.Error);
            }

            var result = await _mediator.Send(new UnbanAccountCommand
            {
                AccountUnbanRequestDto = request
            });

            if (result is { IsFailure: true, Error: not null })
            {
                _logger.LogWarning("Account unban failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while unbanning account: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
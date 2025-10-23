using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class CreateAccountCommand : IRequest<Result<Domain.Model.User>>
{
    public required AccountRegisterRequestDto AccountCreateRequest { get; set; }
    public required string Role { get; set; }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Domain.Model.User>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public CreateAccountCommandHandler(IAccountRepository accountRepository, ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result<Domain.Model.User>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateAccountExists(request);
            if (validationResult.IsFailure && validationResult.Error is not null)
            {
                return Result<Domain.Model.User>.Failure(validationResult.Error);
            }

            var newAccount = new Domain.Model.User
            {
                IdentityId = Guid.NewGuid(),
                Name = request.AccountCreateRequest.Name,
                Surname = request.AccountCreateRequest.Surname,
                Email = request.AccountCreateRequest.Email,
                IdentityNumber = request.AccountCreateRequest.IdentityNumber,
                City = request.AccountCreateRequest.City,
                Country = request.AccountCreateRequest.Country,
                ZipCode = request.AccountCreateRequest.ZipCode,
                Address = request.AccountCreateRequest.Address,
                PhoneNumber = request.AccountCreateRequest.PhoneNumber,
                DateOfBirth = request.AccountCreateRequest.DateOfBirth.ToUniversalTime(),
                Role = request.Role
            };

            await _accountRepository.Create(newAccount, cancellationToken);

            _logger.LogInformation(ErrorMessages.AccountCreated, request.AccountCreateRequest.Email);
            return Result<Domain.Model.User>.Success(newAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountCreationFailed, request.AccountCreateRequest.Email);
            return Result<Domain.Model.User>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<bool>> ValidateAccountExists(CreateAccountCommand request)
    {
        var existingAccountByEmail = await _accountRepository.GetAccountByEmail(request.AccountCreateRequest.Email);
        if (existingAccountByEmail != null)
        {
            return Result<bool>.Failure(ErrorMessages.AccountEmailAlreadyExists);
        }

        var existingAccountByIdentityNumber = await _accountRepository.GetAccountByIdentityNumber(request.AccountCreateRequest.IdentityNumber);
        if (existingAccountByIdentityNumber != null)
        {
            return Result<bool>.Failure(ErrorMessages.IdentityNumberAlreadyExists);
        }

        return Result<bool>.Success(true);
    }
}
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class CreateAccountCommand : IRequest<Result>
{
    public required AccountRegisterRequestDto AccountCreateRequest { get; set; }
    public required string Role { get; set; }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateAccountExists(request);
            if (!validationResult.IsSuccess)
            {
                return Result.Failure(validationResult.Error);
            }
            
            var newAccount = new Domain.Model.Account
            {
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

            await _accountRepository.Create(newAccount);

            _logger.LogInformation("Account created successfully for email: {Email}", request.AccountCreateRequest.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating account for email: {Email}", request.AccountCreateRequest.Email);
            return Result.Failure("An error occurred while processing your request. Please try again later.");
        }
    }

    private async Task<Result> ValidateAccountExists(CreateAccountCommand request)
    {
        var existingAccountByEmail = await _accountRepository.GetAccountByEmail(request.AccountCreateRequest.Email);
        if (existingAccountByEmail != null)
        {
            _logger.LogWarning("Email already exists: {Email}", request.AccountCreateRequest.Email);
            return Result.Failure("Email already exists. Please use a different email address.");
        }

        var existingAccountByIdentityNumber = await _accountRepository.GetAccountByIdentityNumber(request.AccountCreateRequest.IdentityNumber);
        if (existingAccountByIdentityNumber != null)
        {
            _logger.LogWarning("Identity number already exists: {IdentityNumber}", request.AccountCreateRequest.IdentityNumber);
            return Result.Failure("Identity number already exists. Please use a different identity number.");
        }

        _logger.LogInformation("Validation passed for email: {Email}, identityNumber: {IdentityNumber}", 
            request.AccountCreateRequest.Email, request.AccountCreateRequest.IdentityNumber);
        return Result.Success();
    }
}
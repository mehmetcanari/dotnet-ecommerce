using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetClientAccountQuery : IRequest<Result<AccountResponseDto>>{}

public class GetClientAccountQueryHandler : IRequestHandler<GetClientAccountQuery, Result<AccountResponseDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;

    public GetClientAccountQueryHandler(
        IAccountRepository accountRepository,
        ILoggingService logger,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountResponseDto>> Handle(GetClientAccountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = ValidateUser();
            if (!validationResult.IsSuccess)
            {
                return Result<AccountResponseDto>.Failure(validationResult.Error);
            }

            var account = await _accountRepository.GetAccountByEmail(validationResult.Data);
            if (account == null)
            {
                _logger.LogWarning("Account not found for email: {Email}", validationResult.Data);
                return Result<AccountResponseDto>.Failure($"User with email {validationResult.Data} not found");
            }

            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred");
        }
    }

    private Result<string> ValidateUser()
    {
        var emailResult = _currentUserService.GetUserEmail();
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

    private static AccountResponseDto MapToResponseDto(Domain.Model.Account account)
    {
        return new AccountResponseDto
        {
            Id = account.Id,
            Name = account.Name,
            Surname = account.Surname,
            Email = account.Email,
            Address = account.Address,
            PhoneNumber = account.PhoneNumber,
            DateOfBirth = account.DateOfBirth,
            Role = account.Role
        };
    }
}
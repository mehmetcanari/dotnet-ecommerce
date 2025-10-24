using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
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
            if (validationResult.IsFailure && validationResult.Message is not null)
                return Result<AccountResponseDto>.Failure(validationResult.Message);

            if (string.IsNullOrEmpty(validationResult.Data))
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);

            var account = await _accountRepository.GetByEmail(validationResult.Data);
            if (account == null)
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);
            
            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
        }
    }

    private Result<string> ValidateUser()
    {
        var email = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Result<string>.Failure(ErrorMessages.AccountEmailNotFound);

        return Result<string>.Success(email);
    }

    private static AccountResponseDto MapToResponseDto(Domain.Model.User account) => new AccountResponseDto
    {
        Id = account.Id,
        Name = account.Name,
        Surname = account.Surname,
        Email = account.Email ?? string.Empty,
        Address = account.Address,
        PhoneNumber = account.PhoneNumber ?? string.Empty,
        DateOfBirth = account.DateOfBirth,
    };
}
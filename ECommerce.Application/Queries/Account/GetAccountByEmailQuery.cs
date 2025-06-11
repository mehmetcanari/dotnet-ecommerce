using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAccountByEmailQuery : IRequest<Result<AccountResponseDto>>{}

public class GetAccountByEmailQueryHandler : IRequestHandler<GetAccountByEmailQuery, Result<AccountResponseDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;

    public GetAccountByEmailQueryHandler(
        IAccountRepository accountRepository,
        ILoggingService logger,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountResponseDto>> Handle(GetAccountByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result<AccountResponseDto>.Failure(emailResult.Error);
            }

            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null");
                return Result<AccountResponseDto>.Failure("Email is not available");
            }

            var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (account == null)
            {
                return Result<AccountResponseDto>.Failure($"User with email {emailResult.Data} not found");
            }

            var responseDto = new AccountResponseDto
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

            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred");
        }
    }
}
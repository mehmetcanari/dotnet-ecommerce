using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAccountWithIdQuery : IRequest<Result<AccountResponseDto>>
{
    public int Id { get; set; }
}

public class GetAccountWithIdQueryHandler : IRequestHandler<GetAccountWithIdQuery, Result<AccountResponseDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public GetAccountWithIdQueryHandler(IAccountRepository accountRepository, ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result<AccountResponseDto>> Handle(GetAccountWithIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountById(request.Id);
            if (account == null)
            {
                _logger.LogWarning("Account with ID {Id} not found.", request.Id);
                return Result<AccountResponseDto>.Failure($"Account with ID {request.Id} not found.");
            }

            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by ID: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred while fetching the account.");
        }
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
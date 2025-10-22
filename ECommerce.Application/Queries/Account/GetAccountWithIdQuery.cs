using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
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
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
            }

            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
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
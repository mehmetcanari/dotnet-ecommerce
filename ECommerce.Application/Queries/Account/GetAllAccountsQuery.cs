using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAllAccountsQuery : IRequest<Result<List<AccountResponseDto>>>{}

public class GetAllAccountsQueryHandler(IAccountRepository accountRepository, ILogService logger) : IRequestHandler<GetAllAccountsQuery, Result<List<AccountResponseDto>>>
{
    public async Task<Result<List<AccountResponseDto>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await accountRepository.Read(cancellationToken: cancellationToken);
            var accountCount = accounts.Count;
            if (accountCount == 0)
            {
                return Result<List<AccountResponseDto>>.Failure(ErrorMessages.AccountNotFound);
            }
            
            var accountList = accounts.Select(MapToResponseDto).ToList();
            return Result<List<AccountResponseDto>>.Success(accountList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<List<AccountResponseDto>>.Failure(ErrorMessages.AccountNotFound);
        }
    }

    private AccountResponseDto MapToResponseDto(Domain.Model.User account)
    {
        return new AccountResponseDto
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
}
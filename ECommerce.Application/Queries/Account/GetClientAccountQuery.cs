using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetClientAccountQuery : IRequest<Result<AccountResponseDto>>{}

public class GetClientAccountQueryHandler(IAccountRepository accountRepository, ILogService logger, ICurrentUserService currentUserService) : IRequestHandler<GetClientAccountQuery, Result<AccountResponseDto>>
{
    public async Task<Result<AccountResponseDto>> Handle(GetClientAccountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = ValidateUser();
            if (validationResult is { IsFailure: true, Message: not null })
                return Result<AccountResponseDto>.Failure(validationResult.Message);

            if (string.IsNullOrEmpty(validationResult.Data))
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);

            var account = await accountRepository.GetByEmail(validationResult.Data, cancellationToken);
            if (account == null)
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);
            
            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
        }
    }

    private Result<string> ValidateUser()
    {
        var email = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Result<string>.Failure(ErrorMessages.AccountEmailNotFound);

        return Result<string>.Success(email);
    }

    private AccountResponseDto MapToResponseDto(Domain.Model.User account) => new AccountResponseDto
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
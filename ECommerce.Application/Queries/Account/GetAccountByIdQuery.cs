using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAccountByIdQuery(Guid id) : IRequest<Result<AccountResponseDto>>
{
    public readonly Guid UserId = id;
}

public class GetAccountWithIdQueryHandler(IUserRepository userRepository, ILogService logger) : IRequestHandler<GetAccountByIdQuery, Result<AccountResponseDto>>
{
    public async Task<Result<AccountResponseDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await userRepository.GetById(request.UserId, cancellationToken);
            if (account == null)
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);

            var responseDto = MapToResponseDto(account);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
        }
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
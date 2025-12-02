using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.Account;
using ECommerce.Shared.Enum;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAccountByIdQuery(Guid id) : IRequest<Result<AccountResponseDto>>
{
    public readonly Guid UserId = id;
}

public class GetAccountWithIdQueryHandler(IUserRepository userRepository, ILogService logger, ICacheService cache) : IRequestHandler<GetAccountByIdQuery, Result<AccountResponseDto>>
{
    private readonly TimeSpan _ttl = TimeSpan.FromHours(1);

    public async Task<Result<AccountResponseDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedAccount = await cache.GetAsync<AccountResponseDto>($"{CacheKeys.UserAccount}_{request.UserId}", cancellationToken);
            if (cachedAccount is not null)
                return Result<AccountResponseDto>.Success(cachedAccount);

            var account = await userRepository.GetById(request.UserId, cancellationToken);
            if (account == null)
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);

            var response = MapToResponseDto(account);
            await cache.SetAsync($"{CacheKeys.UserAccount}_{request.UserId}", response, CacheExpirationType.Absolute, _ttl, cancellationToken);

            return Result<AccountResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
        }
    }

    private AccountResponseDto MapToResponseDto(User account) => new()
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
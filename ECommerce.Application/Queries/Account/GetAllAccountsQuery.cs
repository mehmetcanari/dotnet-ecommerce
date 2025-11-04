using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAllAccountsQuery(QueryPagination pagination) : IRequest<Result<List<AccountResponseDto>>>
{
    public readonly QueryPagination Pagination = pagination;
}

public class GetAllAccountsQueryHandler(IUserRepository userRepository, ILogService logger, ICacheService cache) : IRequestHandler<GetAllAccountsQuery, Result<List<AccountResponseDto>>>
{
    private readonly string _cacheKey = $"{CacheKeys.AllAccounts}";
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

    public async Task<Result<List<AccountResponseDto>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheItems = await cache.GetAsync<List<AccountResponseDto>>(_cacheKey, cancellationToken);
            if (cacheItems is { Count: > 0 })
                return Result<List<AccountResponseDto>>.Success(cacheItems);

            var users = await userRepository.Read(request.Pagination.Page, request.Pagination.PageSize, cancellationToken);
            if (users.Count == 0)
                return Result<List<AccountResponseDto>>.Failure(ErrorMessages.AccountNotFound);

            var response = users.Select(MapToResponseDto).ToList();
            await cache.SetAsync(_cacheKey, response, CacheExpirationType.Absolute, _expiration, cancellationToken);

            return Result<List<AccountResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<List<AccountResponseDto>>.Failure(ErrorMessages.AccountNotFound);
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

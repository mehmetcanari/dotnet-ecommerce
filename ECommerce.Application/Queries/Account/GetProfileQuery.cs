using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetProfileQuery : IRequest<Result<AccountResponseDto>>;

public class GetProfileQueryHandler(IUserRepository userRepository, ILogService logger, ICurrentUserService currentUserService, ICacheService cacheService) : IRequestHandler<GetProfileQuery, Result<AccountResponseDto>>
{
    private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(1);
    private readonly string _cacheKey = $"{CacheKeys.Profile}_{currentUserService.GetUserId()}";

    public async Task<Result<AccountResponseDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotAuthorized);

            var cachedProfile = await cacheService.GetAsync<AccountResponseDto>(_cacheKey);
            if (cachedProfile != null)
                return Result<AccountResponseDto>.Success(cachedProfile);

            var account = await userRepository.GetById(Guid.Parse(userId), cancellationToken);
            if (account == null)
                return Result<AccountResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);
            
            var responseDto = MapToResponseDto(account);

            await cacheService.SetAsync(_cacheKey, responseDto, ExpirationTime);
            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountNotFound, ex.Message);
            return Result<AccountResponseDto>.Failure(ErrorMessages.AccountNotFound);
        }
    }

    private AccountResponseDto MapToResponseDto(Domain.Model.User account) => new()
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
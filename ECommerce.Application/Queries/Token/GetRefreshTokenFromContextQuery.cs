using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Queries.Token;

public class GetRefreshTokenFromContextQuery : IRequest<Result<RefreshToken>> { }

public class GetRefreshTokenFromContextQueryHandler(IHttpContextAccessor httpContextAccessor, ILogService logService, IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<GetRefreshTokenFromContextQuery, Result<RefreshToken>>
{
    public async Task<Result<RefreshToken>> Handle(GetRefreshTokenFromContextQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Result<RefreshToken>.Failure(ErrorMessages.FailedToFetchUserTokens);

            var token = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
            if (token == null)
                return Result<RefreshToken>.Failure(ErrorMessages.FailedToFetchUserTokens);

            return Result<RefreshToken>.Success(token);
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ErrorMessages.FailedToFetchUserTokens, ex.Message);
            return Result<RefreshToken>.Failure(ex.Message);
        }
    }
}

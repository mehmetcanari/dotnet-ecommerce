using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Token;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Request.Token;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Auth
{
    public class LogoutCommand : IRequest<Result> { }

    public class LogoutCommandHandler(IMediator mediator, ILogService logService, ICurrentUserService currentUserService, IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LogoutCommand, Result>
    {
        private const string Reason = "Logout";

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = currentUserService.GetClientToken();
                if (string.IsNullOrEmpty(token))
                    return Result.Failure(ErrorMessages.NoActiveTokensFound);

                var refreshToken = await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
                if (refreshToken is null)
                    return Result.Failure(ErrorMessages.NoActiveTokensFound);

                var revokeRequest = new TokenRevokeRequestDto { Email = refreshToken.Email, Reason = Reason };
                var revokeResult = await mediator.Send(new RevokeRefreshTokenCommand(revokeRequest), cancellationToken);
                if (revokeResult is { IsFailure: true, Message: not null })
                    return Result.Failure(revokeResult.Message);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.FailedToRevokeToken, ex.Message);
                return Result.Failure(ex.Message);
            }
        }
    }
}

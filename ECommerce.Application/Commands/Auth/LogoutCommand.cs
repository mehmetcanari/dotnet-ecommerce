using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Token;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Queries.Token;
using ECommerce.Application.Utility;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Auth
{
    public class LogoutCommand : IRequest<Result> { }

    public class LogoutCommandHandler(IMediator mediator, ILogService logService) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tokenResult = await mediator.Send(new GetRefreshTokenFromContextQuery(), cancellationToken);
                if(tokenResult is { IsFailure: true , Message: not null})
                    return Result.Failure(tokenResult.Message);

                var refreshToken = tokenResult.Data;
                if(refreshToken is null)
                    return Result.Failure(ErrorMessages.RefreshTokenNotFound);

                var revokeRequest = new TokenRevokeRequestDto { Email = refreshToken.Email, Reason = string.Empty };
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

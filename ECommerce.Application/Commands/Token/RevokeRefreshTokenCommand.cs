using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Request.Token;
using ECommerce.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Commands.Token
{
    public class RevokeRefreshTokenCommand(TokenRevokeRequestDto request) : IRequest<Result>
    {
        public readonly TokenRevokeRequestDto Model = request;
    }

    public class RevokeRefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, ILogService logService, IHttpContextAccessor context, IUnitOfWork unitOfWork) : IRequestHandler<RevokeRefreshTokenCommand, Result>
    {
        public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await refreshTokenRepository.GetActive(request.Model.Email, cancellationToken);
                if (token == null)
                    return Result.Failure(ErrorMessages.NoActiveTokensFound);

                context.HttpContext?.Response.Cookies.Delete("refreshToken");
                refreshTokenRepository.Revoke(token, request.Model.Reason);
                await unitOfWork.Commit();

                return Result.Success();
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.FailedToRevokeToken);
                return Result.Failure(ex.Message);
            }
        }
    }
}

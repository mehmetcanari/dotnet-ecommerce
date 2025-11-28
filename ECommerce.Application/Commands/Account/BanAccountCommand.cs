using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Token;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class BanAccountCommand(AccountBanRequestDto request) : IRequest<Result>
{
    public readonly AccountBanRequestDto Model = request;
}

public class BanAccountCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogService logger, IMediator mediator) : IRequestHandler<BanAccountCommand, Result>
{
    public async Task<Result> Handle(BanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByEmail(request.Model.Email, cancellationToken);
            if (user == null)
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var tokenRevokeRequest = new TokenRevokeRequestDto { Email = request.Model.Email, Reason = ErrorMessages.AccountBanned };
            var revokeResult = await mediator.Send(new RevokeRefreshTokenCommand(tokenRevokeRequest), cancellationToken);
            if (revokeResult is { IsFailure: true })
                return Result.Failure(ErrorMessages.FailedToRevokeToken);

            user.BanAccount(request.Model.Until, request.Model.Reason);
            userRepository.Update(user);
            await unitOfWork.Commit();

            logger.LogInformation(ErrorMessages.AccountBanned, request.Model.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
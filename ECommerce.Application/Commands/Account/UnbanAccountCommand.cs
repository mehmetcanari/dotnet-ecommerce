using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Token;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class UnbanAccountCommand(AccountUnbanRequestDto request) : IRequest<Result>
{
    public readonly AccountUnbanRequestDto Model = request;
}

public class UnbanAccountCommandHandler(IUserRepository userRepository, IMediator mediator, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<UnbanAccountCommand, Result>
{
    public async Task<Result> Handle(UnbanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await userRepository.GetByEmail(request.Model.Email, cancellationToken);
            if (account == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            account.UnbanAccount();
            userRepository.Update(account);

            var tokenRevokeRequest = new TokenRevokeRequestDto { Email = request.Model.Email, Reason = ErrorMessages.AccountUnrestricted };
            var revokeResult = await mediator.Send(new RevokeRefreshTokenCommand(tokenRevokeRequest), cancellationToken);
            if (revokeResult is { IsFailure: true })
                return Result.Failure(ErrorMessages.FailedToRevokeToken);

            await unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
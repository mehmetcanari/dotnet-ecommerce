using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class BanAccountCommand(AccountBanRequestDto request) : IRequest<Result>
{
    public readonly AccountBanRequestDto Model = request;
}

public class BanAccountCommandHandler(IAccountRepository accountRepository, IRefreshTokenService refreshTokenService, ILoggingService logger) : IRequestHandler<BanAccountCommand, Result>
{
    public async Task<Result> Handle(BanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await accountRepository.GetByEmail(request.Model.Email, cancellationToken);
            if (account == null)
                return Result.Failure(ErrorMessages.AccountEmailNotFound);
            
            var tokenRevokeRequest = new TokenRevokeRequestDto { Email = request.Model.Email, Reason = ErrorMessages.AccountBanned };
            await refreshTokenService.RevokeUserTokens(tokenRevokeRequest);

            account.BanAccount(request.Model.Until, request.Model.Reason);
            accountRepository.Update(account);

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
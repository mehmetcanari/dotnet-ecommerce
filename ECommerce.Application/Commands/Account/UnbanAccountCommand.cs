using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class UnbanAccountCommand(AccountUnbanRequestDto request) : IRequest<Result>
{
    public readonly AccountUnbanRequestDto Model = request;
}

public class UnbanAccountCommandHandler(IAccountRepository accountRepository, IRefreshTokenService refreshTokenService, ILoggingService logger) : IRequestHandler<UnbanAccountCommand, Result>
{
    public async Task<Result> Handle(UnbanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await accountRepository.GetByEmail(request.Model.Email, cancellationToken);
            if (account == null)
                return Result.Failure(ErrorMessages.AccountNotFound);
            
            account.UnbanAccount();
            accountRepository.Update(account);

            var tokenRevokeRequest = new TokenRevokeRequestDto
            {
                Email = request.Model.Email, Reason = ErrorMessages.AccountUnrestricted
            };

            await refreshTokenService.RevokeUserTokens(tokenRevokeRequest);

            logger.LogInformation(ErrorMessages.AccountUnrestricted, request.Model.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
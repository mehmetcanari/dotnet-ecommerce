using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class BanAccountCommand : IRequest<Result>
{
    public required AccountBanRequestDto AccountBanRequestDto { get; set; }
}

public class BanAccountCommandHandler : IRequestHandler<BanAccountCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILoggingService _logger;

    public BanAccountCommandHandler(
        IAccountRepository accountRepository,
        IRefreshTokenService refreshTokenService,
        ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(BanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountByEmail(request.AccountBanRequestDto.Email);
            if (account == null)
            {
                return Result.Failure($"Account with email {request.AccountBanRequestDto.Email} not found");
            }
            
            var tokenRevokeRequest = new TokenRevokeRequestDto { Email = request.AccountBanRequestDto.Email, Reason = "Account banned" };
            await _refreshTokenService.RevokeUserTokens(tokenRevokeRequest);

            account.BanAccount(request.AccountBanRequestDto.Until, request.AccountBanRequestDto.Reason);
            _accountRepository.Update(account);

            _logger.LogInformation("Account banned successfully: {Email}", request.AccountBanRequestDto.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while banning the account: {Message}", ex.Message);
            return Result.Failure($"An unexpected error occurred while banning the account: {ex.Message}");
        }
    }
}
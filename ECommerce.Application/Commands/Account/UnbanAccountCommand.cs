using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Account;

public class UnbanAccountCommand : IRequest<Result>
{
    public required AccountUnbanRequestDto AccountUnbanRequestDto { get; set; }
}

public class UnbanAccountCommandHandler : IRequestHandler<UnbanAccountCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILoggingService _logger;

    public UnbanAccountCommandHandler(
        IAccountRepository accountRepository,
        IRefreshTokenService refreshTokenService,
        ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(UnbanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountByEmail(request.AccountUnbanRequestDto.Email);
            if (account == null)
            {
                return Result.Failure("Account not found");
            }
            account.UnbanAccount();
            _accountRepository.Update(account);
            var tokenRevokeRequest = new TokenRevokeRequestDto
            {
                Email = request.AccountUnbanRequestDto.Email, Reason = "Account unbanned successfully by Admin."
            };
            await _refreshTokenService.RevokeUserTokens(tokenRevokeRequest);

            _logger.LogInformation("Account unbanned successfully: {Email}", request.AccountUnbanRequestDto.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while unbanning the account: {Message}", ex.Message);
            return Result.Failure($"An unexpected error occurred while unbanning the account: {ex.Message}");
        }
    }
}
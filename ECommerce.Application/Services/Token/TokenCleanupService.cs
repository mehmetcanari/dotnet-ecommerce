using ECommerce.Application.Abstract.Service;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Token;
public class TokenCleanupService : ITokenCleanupService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _logger;

    public TokenCleanupService(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, ILoggingService logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        try
        {
            await _refreshTokenRepository.CleanupExpiredAsync();
            await _unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.FailedToCleanupExpiredTokens, ex.Message);
            return;
        }
    }
}

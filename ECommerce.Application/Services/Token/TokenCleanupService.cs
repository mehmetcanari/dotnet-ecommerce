using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Abstract.Repository;

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

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshTokenRepository.CleanupExpiredTokensAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Expired tokens cleaned up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired tokens");
            throw new Exception("Failed to cleanup expired tokens", ex);
        }
    }
}

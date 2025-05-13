namespace ECommerce.Application.Interfaces.Service;

public interface ITokenCleanupService
{
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
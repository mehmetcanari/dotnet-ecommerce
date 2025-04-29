using System.Threading;

public interface ITokenCleanupService
{
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}



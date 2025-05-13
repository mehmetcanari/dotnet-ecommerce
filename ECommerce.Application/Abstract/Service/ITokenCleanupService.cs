namespace ECommerce.Application.Abstract.Service;

public interface ITokenCleanupService
{
    Task CleanupExpiredTokensAsync();
}
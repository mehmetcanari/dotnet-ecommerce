using ECommerce.Application.Abstract.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Application.Services.Token;

public class TokenCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TokenCleanupBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanupInterval = TimeSpan.FromHours(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var tokenCleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();
                await tokenCleanupService.CleanupExpiredTokensAsync();
            }

            await Task.Delay(cleanupInterval, stoppingToken);
        }
    }
}

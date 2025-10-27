using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Application.Services.Background;

public class TokenCleanupBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await repository.CleanupExpiredAsync(cancellationToken);
                    await unitOfWork.Commit();
                }

                await Task.Delay(CleanupInterval, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogService>();
            logger.LogError(ex, ex.Message);
        }
    }
}

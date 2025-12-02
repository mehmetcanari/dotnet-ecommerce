using ECommerce.Application.Dependencies;
using ECommerce.Application.Validations;
using ECommerce.Infrastructure.Dependencies;
using ECommerce.Shared.Constants;
using Serilog;
using StackExchange.Redis;

namespace ECommerce.API.Extensions;

public class DependencyContainer(WebApplicationBuilder builder) : IDependencyContainer
{
    public void RegisterDependencies()
    {
        EnvConfig.LoadEnv();

        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        var rabbitMqConnection = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION");

        if (string.IsNullOrEmpty(redisConnectionString))
            throw new InvalidOperationException(ErrorMessages.CacheConnectionStringNotConfigured);

        if (string.IsNullOrEmpty(rabbitMqConnection))
            throw new InvalidOperationException(ErrorMessages.QueueConnectionStringNotConfigured);

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplicationDependencies();
        builder.Services.AddInfrastructureDependencies();
        builder.Services.AddValidationDependencies();
    }
}
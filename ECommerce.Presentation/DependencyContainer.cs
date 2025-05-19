using ECommerce.Application.Dependencies;
using ECommerce.Application.Services.Token;
using ECommerce.Infrastructure.Dependencies;
using Serilog;
using StackExchange.Redis;

namespace ECommerce.API;

public class DependencyContainer : IDependencyContainer
{
    private readonly WebApplicationBuilder _builder;

    public DependencyContainer(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public void RegisterDependencies()
    {
        _builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost:6379"));
        _builder.Services.AddSingleton(Log.Logger);
        _builder.Services.AddHostedService<TokenCleanupBackgroundService>();
        _builder.Services.AddHttpContextAccessor();

        _builder.Services.AddApplicationDependencies();
        _builder.Services.AddInfrastructureDependencies();
        _builder.Services.AddValidationDependencies();
    }
}
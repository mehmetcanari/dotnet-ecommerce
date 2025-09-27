using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Account;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.Dependencies;
using ECommerce.Application.Events;
using ECommerce.Application.Queries.Product;
using ECommerce.Application.Services.Queue;
using ECommerce.Application.Services.Search.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Infrastructure.Dependencies;
using MediatR;
using RabbitMQ.Client;
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
        EnvConfig.LoadEnv();

        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        var rabbitMqConnection = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION");

        _builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        _builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            Uri = new Uri(rabbitMqConnection),
            DispatchConsumersAsync = true
        });

        _builder.Services.AddSingleton<IMessageBroker, RabbitMQService>();
        _builder.Services.AddSingleton(Log.Logger);
        _builder.Services.AddHostedService<TokenCleanupBackgroundService>();
        _builder.Services.AddHttpContextAccessor();

        _builder.Services.AddApplicationDependencies();
        _builder.Services.AddInfrastructureDependencies();
        _builder.Services.AddValidationDependencies();
        RegisterMediatR();
    }

    private void RegisterMediatR()
    {
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductWithIdQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductStockCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateAccountGuidCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientAccountAsEntityQuery).Assembly));
        
        _builder.Services.AddScoped<INotificationHandler<ProductCreatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductStockUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductDeletedEvent>, ProductElasticsearchEventHandler>();
    }
}
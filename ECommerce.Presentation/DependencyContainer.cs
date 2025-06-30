using ECommerce.Application.Dependencies;
using ECommerce.Application.Queries.Product;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Application.Services.Queue;
using ECommerce.Application.Events;
using ECommerce.Infrastructure.Dependencies;
using MediatR;
using Serilog;
using StackExchange.Redis;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Search.Product;

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
        _builder.Services.AddSingleton<IMessageBroker, RabbitMQService>();

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
        
        _builder.Services.AddScoped<INotificationHandler<ProductCreatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductStockUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductDeletedEvent>, ProductElasticsearchEventHandler>();
    }
}
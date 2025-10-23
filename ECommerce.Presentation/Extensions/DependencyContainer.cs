using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Account;
using ECommerce.Application.Commands.Basket;
using ECommerce.Application.Commands.Category;
using ECommerce.Application.Commands.Order;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.Dependencies;
using ECommerce.Application.Events;
using ECommerce.Application.Queries.Account;
using ECommerce.Application.Queries.Basket;
using ECommerce.Application.Queries.Category;
using ECommerce.Application.Queries.Order;
using ECommerce.Application.Queries.Product;
using ECommerce.Application.Services.Queue;
using ECommerce.Application.Services.Search.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Infrastructure.Dependencies;
using ECommerce.Shared.Constants;
using MediatR;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;

namespace ECommerce.API.Extensions;

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

        if(string.IsNullOrEmpty(redisConnectionString))
            throw new InvalidOperationException(ErrorMessages.CacheConnectionStringNotConfigured);

        if(string.IsNullOrEmpty(rabbitMqConnection))
            throw new InvalidOperationException(ErrorMessages.QueueConnectionStringNotConfigured);

        _builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        _builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            Uri = new Uri(rabbitMqConnection),
            DispatchConsumersAsync = true
        });

        _builder.Services.AddSingleton<IMessageBroker, QueueService>();
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
        #region Commands
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BanAccountCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteAccountCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UnbanAccountCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBasketItemCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteAllNonOrderedBasketItemsCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateBasketItemCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteCategoryCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateCategoryCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CancelOrderCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteOrderByIdCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateOrderStatusCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductCommand).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductStockCommand).Assembly));
        #endregion

        #region Queries
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAccountWithIdQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllAccountsQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientAccountAsEntityQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientAccountQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllBasketItemsQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCategoryByIdQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllOrdersQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOrderByIdQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUserOrdersQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
        _builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductSearchQuery).Assembly));
        #endregion

        #region Events
        _builder.Services.AddScoped<INotificationHandler<ProductCreatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductStockUpdatedEvent>, ProductElasticsearchEventHandler>();
        _builder.Services.AddScoped<INotificationHandler<ProductDeletedEvent>, ProductElasticsearchEventHandler>();
        #endregion
    }
}
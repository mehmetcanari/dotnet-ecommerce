using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Account;
using ECommerce.Application.Commands.Auth;
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
using ECommerce.Application.Services.Background;
using ECommerce.Application.Services.Queue;
using ECommerce.Application.Services.Search.Product;
using ECommerce.Application.Validations;
using ECommerce.Infrastructure.Dependencies;
using ECommerce.Shared.Constants;
using MediatR;
using RabbitMQ.Client;
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

        if(string.IsNullOrEmpty(redisConnectionString))
            throw new InvalidOperationException(ErrorMessages.CacheConnectionStringNotConfigured);

        if(string.IsNullOrEmpty(rabbitMqConnection))
            throw new InvalidOperationException(ErrorMessages.QueueConnectionStringNotConfigured);

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            Uri = new Uri(rabbitMqConnection),
            DispatchConsumersAsync = true
        });

        builder.Services.AddSingleton<IMessageBroker, QueueService>();
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddHostedService<TokenCleanupBackgroundService>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplicationDependencies();
        builder.Services.AddInfrastructureDependencies();
        builder.Services.AddValidationDependencies();
        RegisterMediatr();
    }

    private void RegisterMediatr()
    {
        #region Commands
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BanAccountCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteAccountCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UnbanAccountCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBasketItemCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteAllUnorderedBasketItemsCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateBasketItemCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteCategoryCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateCategoryCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CancelOrderCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeleteOrderByIdCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateOrderStatusCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductCommand).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateProductStockCommand).Assembly));
        #endregion

        #region Queries
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAccountByIdQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllAccountsQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientAccountAsEntityQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientAccountQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllBasketItemsQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCategoryByIdQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllOrdersQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOrderByIdQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUserOrdersQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductSearchQuery).Assembly));
        #endregion

        #region Events
        builder.Services.AddScoped<INotificationHandler<ProductCreatedEvent>, ProductElasticsearchEventHandler>();
        builder.Services.AddScoped<INotificationHandler<ProductUpdatedEvent>, ProductElasticsearchEventHandler>();
        builder.Services.AddScoped<INotificationHandler<ProductStockUpdatedEvent>, ProductElasticsearchEventHandler>();
        builder.Services.AddScoped<INotificationHandler<ProductDeletedEvent>, ProductElasticsearchEventHandler>();
        #endregion
    }
}
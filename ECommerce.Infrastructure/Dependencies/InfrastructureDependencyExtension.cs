using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Dependencies;

public static class InfrastructureDependencyExtension
{
    public static void AddInfrastructureDependencies(this IServiceCollection services)
    {
        // PostgreSQL Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBasketItemRepository, BasketItemRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // MongoDB Context
        services.AddSingleton<MongoDbContext>();

        // Unit of Work
        services.AddScoped<IStoreUnitOfWork, StoreUnitOfWork>();
        services.AddScoped<ICrossContextUnitOfWork, CrossContextUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IStoreUnitOfWork>());
    }
}
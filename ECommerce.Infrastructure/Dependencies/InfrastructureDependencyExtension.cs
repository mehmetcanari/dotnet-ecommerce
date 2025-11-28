using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Dependencies;

public static class InfrastructureDependencyExtension
{
    public static void AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBasketItemRepository, BasketItemRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IStoreUnitOfWork, StoreUnitOfWork>();
        services.AddScoped<ICrossContextUnitOfWork, CrossContextUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IStoreUnitOfWork>());
    }
}
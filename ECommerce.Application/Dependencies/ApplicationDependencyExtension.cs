using ECommerce.Application.Abstract;
using ECommerce.Application.Services.Cache;
using ECommerce.Application.Services.Client;
using ECommerce.Application.Services.Elastic;
using ECommerce.Application.Services.Elastic.Descriptors;
using ECommerce.Application.Services.Lock;
using ECommerce.Application.Services.Logging;
using ECommerce.Application.Services.Notification;
using ECommerce.Application.Services.Payment;
using ECommerce.Application.Services.Upload;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Dependencies;

public static class ApplicationDependencyExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<IElasticSearchService, ElasticSearchService>();

        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRealtimeNotificationHandler, RealtimeNotificationHandler>();
        services.AddScoped<ISearchDescriptor<Domain.Model.Product>, ProductSearchDescriptor>();
        services.AddScoped<ILockProvider, InMemoryLockProvider>();

        RegisterMediatr(services);
    }

    private static void RegisterMediatr(IServiceCollection services) => services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyExtension).Assembly));
}
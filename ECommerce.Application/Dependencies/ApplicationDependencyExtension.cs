using ECommerce.Application.Abstract;
using ECommerce.Application.Services.Cache;
using ECommerce.Application.Services.Client;
using ECommerce.Application.Services.Logging;
using ECommerce.Application.Services.Notification;
using ECommerce.Application.Services.Payment;
using ECommerce.Application.Services.Search;
using ECommerce.Application.Services.Upload;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Dependencies;

public static class ApplicationDependencyExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, LogService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IElasticSearchService, ElasticSearchService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRealtimeNotificationHandler, RealtimeNotificationHandler>();
    }
}
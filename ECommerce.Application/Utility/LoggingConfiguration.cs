using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Services.Logging;
using Microsoft.AspNetCore.Builder;

public static class LoggingConfiguration
{
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithEnvironmentUserName()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();
        
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddScoped<ILoggingService, SerilogService>();

        return builder;
    }
}
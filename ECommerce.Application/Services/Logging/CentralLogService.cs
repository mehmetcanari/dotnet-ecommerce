using ILogger = Serilog.ILogger;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.Application.Services.Logging;

public class CentralLogService : ILoggingService
{
    private readonly ILogger _logger;

    public CentralLogService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogInformation(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    public void LogError(Exception ex, string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(ex, messageTemplate, propertyValues);
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        _logger.Warning(messageTemplate, propertyValues);
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        _logger.Debug(messageTemplate, propertyValues);
    }
}
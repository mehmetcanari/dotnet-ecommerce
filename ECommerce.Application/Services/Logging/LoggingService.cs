using ECommerce.Application.Abstract.Service;
using ILogger = Serilog.ILogger;

namespace ECommerce.Application.Services.Logging;

public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;

    public LoggingService(ILogger logger)
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
}
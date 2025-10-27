using ECommerce.Application.Abstract;
using ILogger = Serilog.ILogger;

namespace ECommerce.Application.Services.Logging;

public class LogService(ILogger logger) : ILogService
{
    public void LogInformation(string messageTemplate, params object[] propertyValues) => logger.Information(messageTemplate, propertyValues);

    public void LogError(Exception ex, string messageTemplate, params object[] propertyValues) => logger.Error(ex, messageTemplate, propertyValues);

    public void LogWarning(string messageTemplate, params object[] propertyValues) => logger.Warning(messageTemplate, propertyValues);
}
namespace ECommerce.Application.Interfaces.Service;

public interface ILoggingService
{
    void LogWarning(string message);
    void LogDebug(string message);
    void LogInformation(string messageTemplate, params object[] propertyValues);
    void LogError(Exception ex, string messageTemplate, params object[] propertyValues);
    void LogWarning(string messageTemplate, params object[] propertyValues);
}

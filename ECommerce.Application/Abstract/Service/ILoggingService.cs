namespace ECommerce.Application.Abstract.Service;

public interface ILoggingService
{
    void LogInformation(string messageTemplate, params object[] propertyValues);
    void LogError(Exception ex, string messageTemplate, params object[] propertyValues);
    void LogWarning(string messageTemplate, params object[] propertyValues);
}

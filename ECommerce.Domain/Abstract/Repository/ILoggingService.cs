namespace ECommerce.Domain.Abstract.Repository;

public interface ILoggingService
{
    void LogInformation(string messageTemplate, params object[] propertyValues);
    void LogError(Exception ex, string messageTemplate, params object[] propertyValues);
    void LogWarning(string messageTemplate, params object[] propertyValues);
    void LogDebug(string messageTemplate, params object[] propertyValues);
}

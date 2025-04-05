namespace ECommerce.API;

public interface IDependencyContainer
{
    void RegisterCoreDependencies();
    void LoadValidationDependencies();
}
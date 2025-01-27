namespace OnlineStoreWeb.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        DependencyContainer dependencyContainer = new DependencyContainer();
        dependencyContainer.LoadDependencies(builder);
        dependencyContainer.ValidationDependencies(builder);

        var app = builder.Build();
        
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
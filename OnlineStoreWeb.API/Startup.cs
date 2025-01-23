namespace OnlineStoreWeb.API;

public static class Startup
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        DependencyContainer dependencyContainer = new DependencyContainer();
        dependencyContainer.LoadDependencies(builder);

        var app = builder.Build();

        // app.UseHttpsRedirection(); // TODO: Enable HTTPS redirection, currently no need
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
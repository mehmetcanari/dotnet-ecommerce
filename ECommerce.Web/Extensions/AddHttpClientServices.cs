using ECommerce.Web.Handlers;
using ECommerce.Web.Services;

namespace ECommerce.Web.Extensions;

public static class ServiceRegistration
{
    public static void AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"];

        services.AddHttpContextAccessor();
        services.AddTransient<AuthTokenHandler>();

        void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(apiBaseUrl!);
        }

        services.AddHttpClient<AccountApiService>(ConfigureClient).AddHttpMessageHandler<AuthTokenHandler>();
        services.AddHttpClient<AuthApiService>(ConfigureClient).AddHttpMessageHandler<AuthTokenHandler>();
    }
}
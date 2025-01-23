using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.Product;
using OnlineStoreWeb.API.Services.Account;
using OnlineStoreWeb.API.Services.Order;
using OnlineStoreWeb.API.Services.Product;

namespace OnlineStoreWeb.API;

public class DependencyContainer
{
    public void LoadDependencies(WebApplicationBuilder builder)
    {
        // Repositories
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();

        // Services
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        
        // Database context
        builder.Services.AddDbContext<StoreDbContext>(options =>
            options.UseInMemoryDatabase("StoreDb"));
    }
}
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.DTO.User;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.Product;
using OnlineStoreWeb.API.Services.Account;
using OnlineStoreWeb.API.Services.Order;
using OnlineStoreWeb.API.Services.Product;
using OnlineStoreWeb.API.Validations.Account;
using OnlineStoreWeb.API.Validations.Order;
using OnlineStoreWeb.API.Validations.Product;

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
    
    public void ValidationDependencies(WebApplicationBuilder builder)
    {
        // Validators
        builder.Services.AddFluentValidationAutoValidation();
        
        builder.Services.AddScoped<IValidator<AccountRegisterDto>, AccountRegisterValidation>();
        builder.Services.AddScoped<IValidator<AccountUpdateDto>, AccountUpdateValidation>();
        builder.Services.AddScoped<IValidator<AccountPatchDto>, AccountPartialUpdateValidation>();
        
        builder.Services.AddScoped<IValidator<ProductCreateDto>, ProductCreateValidation>();
        builder.Services.AddScoped<IValidator<ProductUpdateDto>, ProductUpdateValidation>();
        
        builder.Services.AddScoped<IValidator<OrderCreateDto>, OrderCreateValidation>();
        builder.Services.AddScoped<IValidator<OrderUpdateDto>, OrderUpdateValidation>();
    }
}
using FluentValidation;
using FluentValidation.AspNetCore;
using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.DTO.User;
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

public class DependencyContainer : IDependencyContainer
{
    private readonly WebApplicationBuilder _builder;

    public DependencyContainer(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public void RegisterCoreDependencies()
    {
        _builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        _builder.Services.AddScoped<IProductRepository, ProductRepository>();
        _builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        _builder.Services.AddScoped<IAccountService, AccountService>();
        _builder.Services.AddScoped<IOrderService, OrderService>();
        _builder.Services.AddScoped<IProductService, ProductService>();
    }
    
    public void LoadValidationDependencies()
    {
       _builder.Services.AddFluentValidationAutoValidation();
       _builder.Services.AddScoped<IValidator<AccountRegisterDto>, AccountRegisterValidation>();
       _builder.Services.AddScoped<IValidator<AccountUpdateDto>, AccountUpdateValidation>();
       _builder.Services.AddScoped<IValidator<AccountPatchDto>, AccountPartialUpdateValidation>();
       _builder.Services.AddScoped<IValidator<ProductCreateDto>, ProductCreateValidation>();
       _builder.Services.AddScoped<IValidator<ProductUpdateDto>, ProductUpdateValidation>();
       _builder.Services.AddScoped<IValidator<OrderCreateDto>, OrderCreateValidation>();
       _builder.Services.AddScoped<IValidator<OrderUpdateDto>, OrderUpdateValidation>();
    }
}
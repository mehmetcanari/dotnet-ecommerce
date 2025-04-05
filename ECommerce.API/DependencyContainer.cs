using ECommerce.API.DTO.Request.Account;
using ECommerce.API.DTO.Request.Order;
using ECommerce.API.DTO.Request.OrderItem;
using ECommerce.API.DTO.Request.Product;
using ECommerce.API.Repositories.Account;
using ECommerce.API.Repositories.Order;
using ECommerce.API.Repositories.OrderItem;
using ECommerce.API.Repositories.Product;
using ECommerce.API.Services.Account;
using ECommerce.API.Services.Auth;
using ECommerce.API.Services.Order;
using ECommerce.API.Services.OrderItem;
using ECommerce.API.Services.Product;
using ECommerce.API.Services.Token;
using ECommerce.API.Validations.Account;
using ECommerce.API.Validations.Order;
using ECommerce.API.Validations.OrderItem;
using ECommerce.API.Validations.Product;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ECommerce.API;

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
        _builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        _builder.Services.AddScoped<IAccountService, AccountService>();
        _builder.Services.AddScoped<IOrderService, OrderService>();
        _builder.Services.AddScoped<IProductService, ProductService>();
        _builder.Services.AddScoped<IOrderItemService, OrderItemService>();
        _builder.Services.AddScoped<IAuthService, AuthService>();
        _builder.Services.AddScoped<ITokenService, TokenService>();
        
        _builder.Services.AddHttpContextAccessor();
    }
    
    public void LoadValidationDependencies()
    {
       _builder.Services.AddFluentValidationAutoValidation();
       _builder.Services.AddScoped<IValidator<AccountRegisterRequestDto>, AccountRegisterValidation>();
       _builder.Services.AddScoped<IValidator<AccountLoginRequestDto>, AccountLoginValidation>();
       _builder.Services.AddScoped<IValidator<AccountUpdateRequestDto>, AccountUpdateValidation>();
       _builder.Services.AddScoped<IValidator<ProductCreateRequestDto>, ProductCreateValidation>();
       _builder.Services.AddScoped<IValidator<ProductUpdateRequestDto>, ProductUpdateValidation>();
       _builder.Services.AddScoped<IValidator<OrderCreateRequestDto>, OrderCreateValidation>();
       _builder.Services.AddScoped<IValidator<OrderUpdateRequestDto>, OrderUpdateValidation>();
       _builder.Services.AddScoped<IValidator<CreateOrderItemRequestDto>, OrderItemCreateValidation>();
       _builder.Services.AddScoped<IValidator<UpdateOrderItemRequestDto>, OrderItemUpdateValidation>();
    }
}
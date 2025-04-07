using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Request.OrderItem;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Services.Account;
using ECommerce.Application.Services.Auth;
using ECommerce.Application.Services.Order;
using ECommerce.Application.Services.OrderItem;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Application.Validations.Account;
using ECommerce.Application.Validations.Order;
using ECommerce.Application.Validations.OrderItem;
using ECommerce.Application.Validations.Product;
using ECommerce.Infrastructure.Repositories.Account;
using ECommerce.Infrastructure.Repositories.Order;
using ECommerce.Infrastructure.Repositories.OrderItem;
using ECommerce.Infrastructure.Repositories.Product;
using ECommerce.Infrastructure.Repositories.Token;
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
        _builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        _builder.Services.AddScoped<IAccountService, AccountService>();
        _builder.Services.AddScoped<IOrderService, OrderService>();
        _builder.Services.AddScoped<IProductService, ProductService>();
        _builder.Services.AddScoped<IOrderItemService, OrderItemService>();
        _builder.Services.AddScoped<IAuthService, AuthService>();
        _builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
        _builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        
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
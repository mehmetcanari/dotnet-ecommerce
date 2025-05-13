using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Services.Account;
using ECommerce.Application.Services.Auth;
using ECommerce.Application.Services.Cache;
using ECommerce.Application.Services.Logging;
using ECommerce.Application.Services.Order;
using ECommerce.Application.Services.BasketItem;
using ECommerce.Application.Services.Category;
using ECommerce.Application.Services.Payment;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Application.Validations.Account;
using ECommerce.Application.Validations.Order;
using ECommerce.Application.Validations.BasketItem;
using ECommerce.Application.Validations.Category;
using ECommerce.Application.Validations.Payment;
using ECommerce.Application.Validations.Product;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using StackExchange.Redis;

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
        _builder.Services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect("localhost:6379"));
            
        // Repository
        _builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        _builder.Services.AddScoped<IProductRepository, ProductRepository>();
        _builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        _builder.Services.AddScoped<IBasketItemRepository, BasketItemRepository>();
        _builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        _builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        // Service
        _builder.Services.AddSingleton(Log.Logger);
        _builder.Services.AddScoped<IAccountService, AccountService>();
        _builder.Services.AddScoped<IOrderService, OrderService>();
        _builder.Services.AddScoped<IProductService, ProductService>();
        _builder.Services.AddScoped<IBasketItemService, BasketItemService>();
        _builder.Services.AddScoped<IAuthService, AuthService>();
        _builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
        _builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        _builder.Services.AddScoped<ICacheService, RedisCacheService>();
        _builder.Services.AddScoped<ITokenUserClaimsService, TokenUserClaimsService>();
        _builder.Services.AddScoped<ICategoryService, CategoryService>();
        _builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();
        _builder.Services.AddHostedService<TokenCleanupBackgroundService>();
        _builder.Services.AddScoped<IPaymentService, IyzicoPaymentService>();
        _builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Utility
        _builder.Services.AddScoped<ILoggingService, LogService>();
        _builder.Services.AddHttpContextAccessor();
    }
    
    public void LoadValidationDependencies()
    {
       _builder.Services.AddFluentValidationAutoValidation();
       _builder.Services.AddScoped<IValidator<AccountRegisterRequestDto>, AccountRegisterValidation>();
       _builder.Services.AddScoped<IValidator<AccountLoginRequestDto>, AccountLoginValidation>();
       _builder.Services.AddScoped<IValidator<ProductCreateRequestDto>, ProductCreateValidation>();
       _builder.Services.AddScoped<IValidator<ProductUpdateRequestDto>, ProductUpdateValidation>();
       _builder.Services.AddScoped<IValidator<OrderUpdateRequestDto>, OrderUpdateValidation>();
       _builder.Services.AddScoped<IValidator<OrderCreateRequestDto>, OrderCreateValidation>();
       _builder.Services.AddScoped<IValidator<CreateBasketItemRequestDto>, BasketItemCreateValidation>();
       _builder.Services.AddScoped<IValidator<UpdateBasketItemRequestDto>, BasketItemUpdateValidation>();
       _builder.Services.AddScoped<IValidator<CreateCategoryRequestDto>, CategoryCreateValidation>();
       _builder.Services.AddScoped<IValidator<UpdateCategoryRequestDto>, CategoryUpdateValidation>();
       _builder.Services.AddScoped<IValidator<PaymentCard>, PaymentValidation>();
    }
}
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Services.Account;
using ECommerce.Application.Services.Auth;
using ECommerce.Application.Services.BasketItem;
using ECommerce.Application.Services.Cache;
using ECommerce.Application.Services.Category;
using ECommerce.Application.Services.Logging;
using ECommerce.Application.Services.Order;
using ECommerce.Application.Services.Payment;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Services.Token;
using ECommerce.Application.Validations.Account;
using ECommerce.Application.Validations.BasketItem;
using ECommerce.Application.Validations.Category;
using ECommerce.Application.Validations.Order;
using ECommerce.Application.Validations.Payment;
using ECommerce.Application.Validations.Product;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Dependencies;

public static class ApplicationDependencyExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<ILoggingService, LogService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBasketItemService, BasketItemService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITokenUserClaimsService, TokenUserClaimsService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITokenCleanupService, TokenCleanupService>();
        services.AddScoped<IPaymentService, IyzicoPaymentService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPaymentProvider, IyzicoPaymentProvider>();
    }
    
    public static void AddValidationDependencies(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddScoped<IValidator<AccountRegisterRequestDto>, AccountRegisterValidation>();
        services.AddScoped<IValidator<AccountLoginRequestDto>, AccountLoginValidation>();
        services.AddScoped<IValidator<ProductCreateRequestDto>, ProductCreateValidation>();
        services.AddScoped<IValidator<ProductUpdateRequestDto>, ProductUpdateValidation>();
        services.AddScoped<IValidator<OrderUpdateRequestDto>, OrderUpdateValidation>();
        services.AddScoped<IValidator<OrderCreateRequestDto>, OrderCreateValidation>();
        services.AddScoped<IValidator<CreateBasketItemRequestDto>, BasketItemCreateValidation>();
        services.AddScoped<IValidator<UpdateBasketItemRequestDto>, BasketItemUpdateValidation>();
        services.AddScoped<IValidator<CreateCategoryRequestDto>, CategoryCreateValidation>();
        services.AddScoped<IValidator<UpdateCategoryRequestDto>, CategoryUpdateValidation>();
        services.AddScoped<IValidator<PaymentCard>, PaymentValidation>();
    }
}
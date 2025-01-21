using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;
using OnlineStoreWeb.API.Services.Account;
using OnlineStoreWeb.API.Services.Order;
using OnlineStoreWeb.API.Services.OrderItem;
using OnlineStoreWeb.API.Services.Product;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database context
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseInMemoryDatabase("StoreDb"));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();

var app = builder.Build();

// app.UseHttpsRedirection(); // TODO: Enable HTTPS redirection, currently no need
app.UseAuthorization();
app.MapControllers();

app.Run();


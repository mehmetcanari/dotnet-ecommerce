using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API
{
    internal static class Program
    {
        private static IDependencyContainer _dependencyContainer;

        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            _dependencyContainer = new DependencyContainer(builder);

            #region Database Configuration

            builder.Services.AddDbContext<StoreDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("IdentityDb"));
            
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorizationBuilder();
            
            
            //builder.Services.AddHttpLogging(o => { });
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Online Store API",
                    Version = "v1",
                    Description = "A simple Online Store API for managing products"
                });
            });

            _dependencyContainer.RegisterCoreDependencies();
            _dependencyContainer.LoadValidationDependencies();

            var app = builder.Build();
            
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
                await next();
                Console.WriteLine($"Response: {context.Response.StatusCode}");
            });
            
            //app.UseHttpLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Store API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
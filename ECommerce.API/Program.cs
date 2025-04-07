using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using ECommerce.Infrastructure.DatabaseContext;
using System.Threading.RateLimiting;

namespace ECommerce.API
{
    internal static class Program
    {
        private static IDependencyContainer? _dependencyContainer;

        static async Task Main(string[] args)
        {
            //======================================================
            // ENVIRONMENT CONFIGURATION
            // Load environment variables from .env file for secure storage
            // of sensitive information like DB credentials and JWT keys
            //======================================================
            Env.Load();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var builder = WebApplication.CreateBuilder(args);

            //======================================================
            // APPLY ENVIRONMENT VARIABLES
            // Override configuration values with environment variables
            // This ensures sensitive data isn't stored in appsettings.json
            //======================================================
            builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_SECRET");
            builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
            builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            builder.Configuration["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            _dependencyContainer = new DependencyContainer(builder);

            #region Database Configuration

            //======================================================
            // DATABASE SETUP
            // Configure the database contexts for the application
            // Currently using in-memory database for development
            //======================================================

            // PostgreSQL configuration using environment variable directly
            builder.Services.AddDbContext<StoreDbContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

            /* builder.Services.AddDbContext<StoreDbContext>(options =>
                options.UseInMemoryDatabase("StoreDb")); */

            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

            //======================================================
            // IDENTITY CONFIGURATION
            // Set up ASP.NET Core Identity for authentication and authorization
            //======================================================
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            #region JWT Authentication

            //======================================================
            // JWT AUTHENTICATION SETUP
            // Configure JWT-based authentication with token validation parameters
            // Uses the JWT_SECRET from environment variables for security
            //======================================================
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"] ?? throw new Exception("JWT_SECRET is not set");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };
                });


            builder.Services.AddAuthorization();

            #endregion

            #region Swagger Configuration

            //======================================================
            // SWAGGER/OPENAPI CONFIGURATION
            // Set up Swagger documentation with JWT authentication support
            //======================================================
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Online Store API",
                    Version = "v1",
                    Description = "A simple Online Store API for managing products"
                });

                // Add JWT Authentication to Swagger
                var securitySchema = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT token as: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                });
            });

            #endregion

            //======================================================
            // CORE SERVICES REGISTRATION
            // Add controllers and register custom dependencies
            //======================================================
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            //======================================================
            // RATE LIMITING CONFIGURATION
            // Configure rate limiting middleware
            //======================================================

            builder.Services.AddRateLimiter(options =>
            {
                // Global rate limiting policy
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));
                
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Status = 429,
                        Message = "Too many requests. Please try again later.",
                        RetryAfter = "60 seconds"
                    }, cancellationToken);
                };
            });

            _dependencyContainer.RegisterCoreDependencies();
            _dependencyContainer.LoadValidationDependencies();

            var app = builder.Build();

            #region Initialize Roles

            //======================================================
            // ROLE INITIALIZATION
            // Create default roles (Admin, User) if they don't exist
            //======================================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await InitializeRoles(services);
            }

            #endregion

            #region Middleware Configuration

            //======================================================
            // MIDDLEWARE CONFIGURATION
            // Configure the HTTP request pipeline with logging, authentication, etc.
            //======================================================
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
                await next();
                Console.WriteLine($"Response: {context.Response.StatusCode}");
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Store API v1");
                    c.RoutePrefix = string.Empty;
                });
            }
            
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            #endregion

            await app.RunAsync();
        }

        //======================================================
        // ROLE INITIALIZATION METHOD
        // Helper method to create default roles in the system
        //======================================================
        private static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
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
using Asp.Versioning;
using Serilog;
using Serilog.Events;

namespace ECommerce.API
{
    internal static class Program
    {
        private static IDependencyContainer? _dependencyContainer;

        static async Task Main(string[] args)
        {
            // Load environment variables from the root .env file
            var rootPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
            Env.Load(rootPath);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var builder = WebApplication.CreateBuilder(args);

            //======================================================
            // APPLY ENVIRONMENT VARIABLES
            //======================================================
            
            builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_SECRET");
            builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
            builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            builder.Configuration["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            
            _dependencyContainer = new DependencyContainer(builder);
            _dependencyContainer.RegisterCoreDependencies();
            _dependencyContainer.LoadValidationDependencies();

            #region Database Configuration

            //======================================================
            // DATABASE SETUP
            //======================================================

            builder.Services.AddDbContext<StoreDbContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

            //======================================================
            // IDENTITY CONFIGURATION
            //======================================================
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            #region JWT Authentication

            //======================================================
            // JWT AUTHENTICATION SETUP
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
            //======================================================
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Online Store API",
                    Version = "v1",
                    Description = "A simple Online Store API for managing products"
                });

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

            #region Serilog Configuration

            //======================================================
            // SERILOG IMPLEMENTATION
            //======================================================
            builder.Host.UseSerilog((_, _, configuration) => configuration
                .MinimumLevel.Debug()                           
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)  
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("Logs/log-{Date}.log", 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithEnvironmentUserName());

            #endregion

            #region API Versioning Configuration

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            });

            #endregion

            #region Rate Limiting Configuration

            //======================================================
            // RATE LIMITING CONFIGURATION
            //======================================================

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
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

            #endregion

            #region Redis Configuration
            //======================================================
            // REDIS CACHE CONFIGURATION
            //======================================================
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
                options.InstanceName = "ECommerce.API";
            });
            #endregion

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            #region Database Migration

            //======================================================
            // DOCKER DATABASE MIGRATION
            //======================================================
            if (args.Contains("--migrate"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var storeContext = services.GetRequiredService<StoreDbContext>();
                    var identityContext = services.GetRequiredService<ApplicationIdentityDbContext>();

                    try
                    {
                        await using var connection = identityContext.Database.GetDbConnection();
                        await connection.OpenAsync();
                        await using var command = connection.CreateCommand();
                        command.CommandText = "CREATE SCHEMA IF NOT EXISTS \"Identity\";";
                        await command.ExecuteNonQueryAsync();
                        await identityContext.Database.MigrateAsync();
                        await storeContext.Database.MigrateAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("An error occurred while migrating the database.", ex);
                    }
                }
                return;
            }

            #endregion

            #region Initialize Roles

            //======================================================
            // ROLE INITIALIZATION
            //======================================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await InitializeRoles(services);
            }

            #endregion

            #region App Configurations

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Store API v1");
                    c.RoutePrefix = string.Empty;
                });
            }
            
            app.UseSerilogRequestLogging();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // HTTPS Redirection 
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                await next();
            });

            #endregion

            app.Run();
        }

        private static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
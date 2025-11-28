using Amazon.Runtime;
using Amazon.S3;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DotNetEnv;
using ECommerce.API.Configurations;
using ECommerce.API.Extensions;
using ECommerce.API.Middlewares;
using ECommerce.Application.Services.Notification;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;

namespace ECommerce.API;

internal static class Program
{
    private static IDependencyContainer _dependencyContainer = null!;

    private static async Task Main(string[] args)
    {
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(rootPath))
        {
            rootPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
        }
        Env.Load(rootPath);

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var builder = WebApplication.CreateBuilder(args);

        //======================================================
        // APPLY ENVIRONMENT VARIABLES
        //======================================================

        var requiredEnvVars = new Dictionary<string, string?>
        {
            ["JWT_SECRET"] = Environment.GetEnvironmentVariable("JWT_SECRET"),
            ["JWT_ISSUER"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ["JWT_AUDIENCE"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ["DB_CONNECTION_STRING"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
            ["AWS_ACCESS_KEY"] = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY"),
            ["AWS_SECRET_KEY"] = Environment.GetEnvironmentVariable("AWS_SECRET_KEY"),
            ["AWS_REGION"] = Environment.GetEnvironmentVariable("AWS_REGION"),
            ["AWS_BUCKET_NAME"] = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME")
        };

        foreach (var envVar in requiredEnvVars.Where(kv => string.IsNullOrEmpty(kv.Value)))
        {
            throw new InvalidOperationException($"Required environment variable '{envVar.Key}' is not set.");
        }

        builder.Configuration["Jwt:Key"] = requiredEnvVars["JWT_SECRET"];
        builder.Configuration["Jwt:Issuer"] = requiredEnvVars["JWT_ISSUER"];
        builder.Configuration["Jwt:Audience"] = requiredEnvVars["JWT_AUDIENCE"];
        builder.Configuration["AWS:AccessKey"] = requiredEnvVars["AWS_ACCESS_KEY"];
        builder.Configuration["AWS:SecretKey"] = requiredEnvVars["AWS_SECRET_KEY"];
        builder.Configuration["AWS:Region"] = requiredEnvVars["AWS_REGION"];
        builder.Configuration["ConnectionStrings:DefaultConnection"] = requiredEnvVars["DB_CONNECTION_STRING"];
        builder.Configuration["AWS:BucketName"] = requiredEnvVars["AWS_BUCKET_NAME"];

        _dependencyContainer = new DependencyContainer(builder);
        _dependencyContainer.RegisterDependencies();

        #region AWS S3 Configuration

        //======================================================
        // AWS S3 CONFIGURATION
        //======================================================

        var awsCredentials = new BasicAWSCredentials(
            Environment.GetEnvironmentVariable("AWS_ACCESS_KEY"),
            Environment.GetEnvironmentVariable("AWS_SECRET_KEY")
        );

        builder.Services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION")),
                ForcePathStyle = true
            };

            return new AmazonS3Client(awsCredentials, config);
        });

        #endregion

        #region Database Configuration

        //======================================================
        // DATABASE SETUP
        //======================================================

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        builder.Services.AddDbContext<StoreDbContext>(options =>
        {
            options.UseNpgsql(requiredEnvVars["DB_CONNECTION_STRING"]);
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(requiredEnvVars["DB_CONNECTION_STRING"]);
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        //======================================================
        // IDENTITY CONFIGURATION
        //======================================================
        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
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
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
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
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/notificationHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("User", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("User", "Admin");
            });

            options.AddPolicy("Admin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });
        });

        #endregion

        #region Swagger Configuration

        //======================================================
        // SWAGGER/OPENAPI CONFIGURATION
        //======================================================
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen(options =>
        {
            options.OperationFilter<FileUploadOperationFilter>();
        });

        #endregion

        #region Serilog Configuration

        //======================================================
        // SERILOG IMPLEMENTATION 
        //======================================================
        builder.Host.UseSerilog((context, _, configuration) =>
        {
            var env = context.HostingEnvironment;

            configuration
                .MinimumLevel.Is(env.IsDevelopment()
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("Logs/log-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        });

        #endregion

        #region Elasticsearch Configuration
        //======================================================
        // ELASTICSEARCH CONFIGURATION 
        //======================================================

        var elasticUri = "http://localhost:9200";

        builder.Services.AddSingleton(_ =>
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticUri));

            return new ElasticsearchClient(settings);
        });

        #endregion

        #region SignalR Configuration

        //======================================================
        // SIGNALR CONFIGURATION
        //======================================================
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 1024 * 1024;
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        #endregion

        #region API Versioning Configuration

        //======================================================
        // API VERSIONING CONFIGURATION
        //======================================================
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new QueryStringApiVersionReader("version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
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
                    partitionKey: context.User.Identity?.Name ??
                                 context.Connection.RemoteIpAddress?.ToString() ??
                                 context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddFixedWindowLimiter("AuthPolicy", configureOptions =>
            {
                configureOptions.AutoReplenishment = true;
                configureOptions.PermitLimit = 5;
                configureOptions.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter("ApiPolicy", configureOptions =>
            {
                configureOptions.AutoReplenishment = true;
                configureOptions.PermitLimit = 60;
                configureOptions.Window = TimeSpan.FromMinutes(1);
            });

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
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6380";
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = { redisConnectionString },
            AbortOnConnectFail = false,
            ConnectRetry = 3,
            ConnectTimeout = 5000
        };

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = configurationOptions;
            options.InstanceName = "ECommerce.Cache";
        });

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configurationOptions));

        builder.Services.AddScoped<IDatabase>(sp =>
        {
            var connection = sp.GetRequiredService<IConnectionMultiplexer>();
            return connection.GetDatabase();
        });


        #endregion

        #region Health Checks

        //======================================================
        // HEALTH CHECKS
        //======================================================
        builder.Services.AddHealthChecks()
            .AddNpgSql(requiredEnvVars["DB_CONNECTION_STRING"]!, name: "postgresql")
            .AddRedis(redisConnectionString, name: "redis");

        #endregion

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("default", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

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

        #region Database Seeding
        DatabaseSeeder.SeedDatabaseAsync(app).GetAwaiter().GetResult();
        #endregion

        #region Middleware Pipeline 

        //======================================================
        // MIDDLEWARE PIPELINE 
        //======================================================

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.Use(async (context, next) =>
        {
            var response = context.Response;

            response.Headers.Append("Content-Security-Policy",
                "default-src 'none'; " +
                "script-src 'self'; " +
                "connect-src 'self' http://localhost:3000 http://localhost:3002 http://localhost:5076 ws://localhost:5076; " +
                "img-src 'self' data:; " +
                "style-src 'self' 'unsafe-inline'; " +
                "base-uri 'self'; " +
                "form-action 'self'");

            response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            response.Headers.Append("X-Content-Type-Options", "nosniff");
            response.Headers.Append("X-Frame-Options", "DENY");
            response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

            await next();
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions.Reverse())
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"E-Commerce API {description.GroupName.ToUpperInvariant()}"
                    );
                }
                options.RoutePrefix = string.Empty;
                options.DisplayRequestDuration();
                options.DocExpansion(DocExpansion.List);
                options.DefaultModelsExpandDepth(-1);
            });
        }

        app.UseRateLimiter();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown");
            };
        });

        app.UseCors();
        app.UseWebSockets();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<NotificationHub>("/notificationHub").RequireCors("default").RequireAuthorization();

        app.MapHealthChecks("/health");

        #endregion

        Console.WriteLine($"E-Commerce API starting on {app.Environment.EnvironmentName} environment");
        Console.WriteLine("Health checks available at: /health");

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("API Documentation available at the root URL (e.g., http://localhost:5076/)");
        }

        await app.RunAsync();
    }

    private static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            string[] roleNames = ["Admin", "User"];

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    Console.WriteLine(result.Succeeded
                        ? $"Role '{roleName}' created successfully."
                        : $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing roles: {ex.Message}");
            throw;
        }
    }
}
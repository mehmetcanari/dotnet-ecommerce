using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using System.Threading.RateLimiting;
using Asp.Versioning;
using ECommerce.Application.Exceptions;
using ECommerce.Infrastructure.Context;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.RateLimiting;
using Amazon.S3;
using Amazon.Runtime;

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
        
        // Validate required environment variables
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

        builder.Services.AddSingleton<IAmazonS3>(sp =>
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

        builder.Services.AddDbContext<StoreDbContext>(options =>
        {
            options.UseNpgsql(requiredEnvVars["DB_CONNECTION_STRING"]);
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
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
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Password settings for production
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
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
                    // Additional security
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };
                
                // JWT Bearer events for better error handling
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
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
                Title = "E-Commerce API",
                Version = "v1",
                Description = "Modern e-commerce RESTful API with Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "bsn.mehmetcanari@gmail.com"
                }
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
                { securitySchema, ["Bearer"] }
            });
        });

        #endregion

        #region Serilog Configuration

        //======================================================
        // SERILOG IMPLEMENTATION
        //======================================================
        builder.Host.UseSerilog((context, _, configuration) => configuration
            .MinimumLevel.Debug()                           
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("Logs/log-{Date}.log", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName());

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
            // Global rate limit
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? 
                                 context.Connection.RemoteIpAddress?.ToString() ?? 
                                 context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Specific policies for different endpoints
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
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
        
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "ECommerce.Cache";
            options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                EndPoints = { redisConnectionString },
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000
            };
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

        var app = builder.Build();

        #region Database Migration

        //======================================================
        // DOCKER DATABASE MIGRATION
        //======================================================
        if (args.Contains("--migrate"))
        {
            var scope = app.Services.CreateScope();
            try
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
                    
                    Console.WriteLine("Database migration completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database migration failed: {ex.Message}");
                    throw new Exception("An error occurred while migrating the database.", ex);
                }

                return;
            }
            finally
            {
                scope.Dispose();
            }
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

        #region Middleware Pipeline 

        //======================================================
        // MIDDLEWARE PIPELINE - PROPER ORDER
        //======================================================
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Use(async (context, next) =>
        {
            var response = context.Response;
            
            response.Headers.Append("Content-Security-Policy", 
                "default-src 'none'; " +
                "script-src 'self'; " +
                "connect-src 'self'; " +
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
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
                c.RoutePrefix = string.Empty;
                c.DisplayRequestDuration();
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

        app.UseAuthentication();
        app.UseAuthorization();
    
        app.MapHealthChecks("/health");

        app.MapControllers();

        #endregion

        Console.WriteLine($"🚀 E-Commerce API starting on {app.Environment.EnvironmentName} environment");
        Console.WriteLine($"📊 Health checks available at: /health");
        
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"📖 API Documentation: http://localhost:5076");
        }

        await app.RunAsync();
    }

    private static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = ["Admin", "User"];

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"✅ Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing roles: {ex.Message}");
            throw;
        }
    }
}
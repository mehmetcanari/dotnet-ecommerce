using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ECommerce.API.Configurations
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly ILogger<ConfigureSwaggerOptions> _logger;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, ILogger<ConfigureSwaggerOptions> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public void Configure(SwaggerGenOptions options)
        {
            _logger.LogDebug("Discovered API versions: {ApiVersions}", string.Join(", ", _provider.ApiVersionDescriptions.Select(d => d.ApiVersion)));

            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
            }

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        }

        /// <summary>
        /// Creates an OpenApiInfo object for the specified API version.
        /// </summary>
        /// <param name="description">API version description.</param>
        /// <returns>OpenApiInfo object.</returns>
        private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "E-Commerce API",
                Version = description.ApiVersion.ToString(),
                Description = "Modern e-commerce RESTful API with Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "bsn.mehmetcanari@gmail.com",
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " **This API version has been deprecated.**";
            }

            return info;
        }
    }
}
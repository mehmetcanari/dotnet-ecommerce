using Asp.Versioning.ApiExplorer;
using ECommerce.Application.Abstract;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ECommerce.API.Configurations
{
    public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, ILogService logger) : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            logger.LogInformation("Discovered API versions: {ApiVersions}", string.Join(", ", provider.ApiVersionDescriptions.Select(d => d.ApiVersion)));

            foreach (var description in provider.ApiVersionDescriptions)
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
                    }, []
                }
            });
        }

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
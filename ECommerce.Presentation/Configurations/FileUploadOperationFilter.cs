using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerce.API.SwaggerFilters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(IFormFile));

            if (formFileParameters.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = formFileParameters.ToDictionary(
                                    p => p.Name,
                                    p => new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    })
                            }
                        }
                    }
                };

                foreach (var parameter in formFileParameters)
                {
                    var parameterToRemove = operation.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                    if (parameterToRemove != null)
                    {
                        operation.Parameters.Remove(parameterToRemove);
                    }
                }
            }
        }
    }
} 
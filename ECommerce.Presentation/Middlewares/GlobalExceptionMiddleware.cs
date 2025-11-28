using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ECommerce.API.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new
            {
                e.PropertyName,
                e.ErrorMessage
            });

            var json = JsonSerializer.Serialize(new
            {
                isSuccess = false,
                isFailure = true,
                errors
            });

            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(new
            {
                isSuccess = false,
                isFailure = true,
                error = ex.Message
            });

            await context.Response.WriteAsync(json);
        }
    }
}
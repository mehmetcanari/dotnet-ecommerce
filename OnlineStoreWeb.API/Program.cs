using System.Text.Json;
using Microsoft.OpenApi.Models;
using OnlineStoreWeb.API;

var builder = WebApplication.CreateBuilder(args);

//#region Service Configuration
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

DependencyContainer dependencyContainer = new DependencyContainer();
dependencyContainer.LoadDependencies(builder);
dependencyContainer.ValidationDependencies(builder);

var app = builder.Build();

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
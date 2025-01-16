var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

DependencyInjection.AddDependencyInjection(builder.Services);

var app = builder.Build();
app.Run();


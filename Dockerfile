# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY ECommerce.API/*.csproj ./ECommerce.API/
COPY ECommerce.Application/*.csproj ./ECommerce.Application/
COPY ECommerce.Domain/*.csproj ./ECommerce.Domain/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
RUN dotnet restore

# Copy the rest of the files and build the application
COPY . .

# Build Infrastructure project first to ensure migrations are compiled
WORKDIR /app/ECommerce.Infrastructure
RUN dotnet build -c Release

# Build and publish API
WORKDIR /app/ECommerce.API
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/ECommerce.API/out .
COPY --from=build /app/ECommerce.Infrastructure/bin/Release/net9.0/ECommerce.Infrastructure.dll ./
COPY --from=build /app/ECommerce.Infrastructure/Migrations ./Migrations

# Ensure the migration command is supported by your API
RUN echo '#!/bin/sh\n\
export DOTNET_ENVIRONMENT=Development\n\
dotnet ECommerce.API.dll --migrate\n\
dotnet ECommerce.API.dll' > /app/entrypoint.sh && \
chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Add labels
LABEL maintainer="ECommerce Team"
LABEL description="ECommerce API Service"
LABEL version="1.0"

# Copy solution and project files
COPY *.sln .
COPY ECommerce.Presentation/*.csproj ./ECommerce.Presentation/
COPY ECommerce.Application/*.csproj ./ECommerce.Application/
COPY ECommerce.Domain/*.csproj ./ECommerce.Domain/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
COPY ECommerce.Tests/*.csproj ./ECommerce.Tests/

# Restore packages
RUN dotnet restore

# Copy the rest of the files and build the application
COPY . .

# Build Infrastructure project first to ensure migrations are compiled
WORKDIR /app/ECommerce.Infrastructure
RUN dotnet build -c Release

# Build and publish API
WORKDIR /app/ECommerce.Presentation
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Add non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy only the necessary files
COPY --from=build /app/ECommerce.Presentation/out .
COPY --from=build /app/ECommerce.Infrastructure/bin/Release/net9.0/ECommerce.Infrastructure.dll ./
COPY --from=build /app/ECommerce.Infrastructure/Migrations ./Migrations

# Create and set up entrypoint script
RUN echo '#!/bin/sh\n\
export DOTNET_ENVIRONMENT=Development\n\
echo "Waiting for database to be ready..."\n\
sleep 10\n\
echo "Running database migrations..."\n\
dotnet ECommerce.Presentation.dll --migrate\n\
echo "Starting application..."\n\
dotnet ECommerce.Presentation.dll' > /app/entrypoint.sh && \
chmod +x /app/entrypoint.sh

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose the port
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
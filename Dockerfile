FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

LABEL maintainer="ECommerce"
LABEL description="ECommerce"
LABEL version="1.0"

COPY *.sln .
COPY ECommerce.Presentation/*.csproj ./ECommerce.Presentation/
COPY ECommerce.Application/*.csproj ./ECommerce.Application/
COPY ECommerce.Domain/*.csproj ./ECommerce.Domain/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
COPY ECommerce.Tests/*.csproj ./ECommerce.Tests/

RUN dotnet restore

COPY . .

WORKDIR /app/ECommerce.Infrastructure
RUN dotnet build -c Release

WORKDIR /app/ECommerce.Presentation
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/ECommerce.Presentation/out .
COPY --from=build /app/ECommerce.Infrastructure/bin/Release/net9.0/ECommerce.Infrastructure.dll ./
COPY --from=build /app/ECommerce.Infrastructure/Migrations ./Migrations

RUN echo '#!/bin/sh\n\
export DOTNET_ENVIRONMENT=Development\n\
echo "Waiting for database to be ready..."\n\
sleep 10\n\
echo "Running database migrations..."\n\
dotnet ECommerce.Presentation.dll --migrate\n\
echo "Starting application..."\n\
dotnet ECommerce.Presentation.dll' > /app/entrypoint.sh && \
chmod +x /app/entrypoint.sh

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
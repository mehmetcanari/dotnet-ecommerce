FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

LABEL maintainer="ECommerce"
LABEL description="ECommerce"
LABEL version="1.0"

COPY *.sln ./
COPY ECommerce.Presentation/*.csproj ./ECommerce.Presentation/
COPY ECommerce.Application/*.csproj ./ECommerce.Application/
COPY ECommerce.Domain/*.csproj ./ECommerce.Domain/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
COPY ECommerce.Tests/*.csproj ./ECommerce.Tests/

RUN dotnet restore ./ECommerce.sln

COPY . .

WORKDIR /app/ECommerce.Presentation
RUN dotnet publish -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/out ./
COPY --from=build /app/ECommerce.Infrastructure/bin/Release/net9.0/ECommerce.Infrastructure.dll ./

ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
ENV TZ=Europe/Istanbul

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerce.Presentation.dll"]

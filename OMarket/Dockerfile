# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base

RUN apt-get update && apt-get install -y curl

RUN if ! getent group appgroup > /dev/null 2>&1; then \
    addgroup --system appgroup; \
    fi

RUN if ! id -u app > /dev/null 2>&1; then \
    adduser --system --ingroup appgroup --uid 1000 app; \
    fi

RUN mkdir -p /app/.aspnet/DataProtection-Keys /app/logs /app/wwwroot/Static && \
    chown -R app:appgroup /app/.aspnet /app/logs /app/wwwroot && \
    chmod -R 775 /app/.aspnet /app/logs /app/wwwroot

USER app
WORKDIR /app
EXPOSE 7558

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=1

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OMarket/OMarket.csproj", "OMarket/"]
RUN dotnet restore "./OMarket/OMarket.csproj"
COPY . .
WORKDIR "/src/OMarket"
RUN dotnet build "./OMarket.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OMarket.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OMarket.dll"]
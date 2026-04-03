# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and all project files first (layer-cache friendly)
COPY SocialMarketplace.sln .
COPY Marketplace.Api/Marketplace.Api.csproj Marketplace.Api/
COPY Marketplace.Core/Marketplace.Core.csproj Marketplace.Core/
COPY Marketplace.Database/Marketplace.Database.csproj Marketplace.Database/
COPY Marketplace.Orchestrator/Marketplace.Orchestrator.csproj Marketplace.Orchestrator/
COPY Marketplace.Realtime/Marketplace.Realtime.csproj Marketplace.Realtime/
COPY Marketplace.Slices/Marketplace.Slices.csproj Marketplace.Slices/
COPY Marketplace.Workers/Marketplace.Workers.csproj Marketplace.Workers/

# Restore NuGet packages (cached unless .csproj files change)
RUN dotnet restore

# Copy all source code and publish
COPY . .
WORKDIR /src/Marketplace.Api
RUN dotnet publish -c Release --no-restore -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for health checks (Railway uses HTTP health probes)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create logs directory with correct permissions before switching user
RUN mkdir -p /app/logs

# Create non-root user
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup --home /app appuser && \
    chown -R appuser:appgroup /app

USER appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

# Railway injects PORT; fall back to 8080 for local Docker runs
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "Marketplace.Api.dll"]

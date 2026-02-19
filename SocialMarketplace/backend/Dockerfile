# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY SocialMarketplace.sln .
COPY Marketplace.Database/Marketplace.Database.csproj Marketplace.Database/
COPY Marketplace.Core/Marketplace.Core.csproj Marketplace.Core/
COPY Marketplace.Slices/Marketplace.Slices.csproj Marketplace.Slices/
COPY Marketplace.Orchestrator/Marketplace.Orchestrator.csproj Marketplace.Orchestrator/
COPY Marketplace.Workers/Marketplace.Workers.csproj Marketplace.Workers/
COPY Marketplace.Api/Marketplace.Api.csproj Marketplace.Api/
COPY Marketplace.Realtime/Marketplace.Realtime.csproj Marketplace.Realtime/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/Marketplace.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Create non-root user
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Marketplace.Api.dll"]

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Marketplace.Core.Infrastructure;
using Marketplace.Core.Caching;
using Marketplace.Core.Performance;
using StackExchange.Redis;

namespace Marketplace.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketplaceCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection Factory
        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection));

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<IAdaptiveCache, AdaptiveCache>();

        // Infrastructure
        services.AddSingleton<IQuantumRouter, QuantumRouter>();
        services.AddSingleton<IJobQueue, RedisJobQueue>();
        services.AddScoped<IEntanglementManager, EntanglementManager>();

        // Performance
        services.AddSingleton<IComputePool, ComputePool>();
        services.AddSingleton<IDistributedLock, DistributedLock>();

        return services;
    }
}

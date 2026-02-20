using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        // Redis (optional - gracefully degrades without it)
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RedisJobQueue>>();
            try
            {
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 3000;
                options.SyncTimeout = 2000;
                return ConnectionMultiplexer.Connect(options);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis not available — running without distributed cache/queues");
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            }
        });

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

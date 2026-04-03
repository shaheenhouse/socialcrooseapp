using Microsoft.Extensions.Caching.Memory;
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

        // Redis (optional — set Redis:Enabled to false in appsettings.Development when Redis is not installed)
        var redisEnabled = configuration.GetValue("Redis:Enabled", true);
        var redisConnection = configuration.GetConnectionString("Redis");
        if (redisEnabled && string.IsNullOrWhiteSpace(redisConnection))
            redisConnection = "localhost:6379";

        services.AddMemoryCache();

        if (redisEnabled)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnection!);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 3000;
                options.SyncTimeout = 2000;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddSingleton<IAdaptiveCache, AdaptiveCache>();
            services.AddSingleton<IJobQueue, RedisJobQueue>();
            services.AddSingleton<IDistributedLock, DistributedLock>();
        }
        else
        {
            services.AddSingleton<IAdaptiveCache>(sp => new AdaptiveCache(
                sp.GetRequiredService<IMemoryCache>(),
                redis: null,
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<AdaptiveCache>>()));
            services.AddSingleton<IJobQueue, NullJobQueue>();
            services.AddSingleton<IDistributedLock, NoOpDistributedLock>();
        }

        // Infrastructure
        services.AddSingleton<IQuantumRouter, QuantumRouter>();
        services.AddScoped<IEntanglementManager, EntanglementManager>();

        // Performance
        services.AddSingleton<IComputePool, ComputePool>();

        return services;
    }
}

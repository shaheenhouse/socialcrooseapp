using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Realtime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeServices(this IServiceCollection services, string? redisConnection = null)
    {
        var signalR = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            try
            {
                signalR.AddStackExchangeRedis(redisConnection, options =>
                {
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("marketplace:");
                    options.Configuration.AbortOnConnectFail = false;
                });
            }
            catch
            {
                // Redis not available, SignalR works fine in single-server mode
            }
        }

        return services;
    }
}

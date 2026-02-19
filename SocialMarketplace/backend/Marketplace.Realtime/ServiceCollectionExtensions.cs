using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Marketplace.Realtime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeServices(this IServiceCollection services, string redisConnection)
    {
        services.AddSignalR()
            .AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("marketplace:");
            });

        return services;
    }
}

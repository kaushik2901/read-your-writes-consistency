using StackExchange.Redis;

namespace ReadYourWritesConsistency.API.Caching;

public static class RedisConnectionMultiplexerExtension
{
    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new ArgumentException("Redis connection string is not configured");
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        return services;
    }
}

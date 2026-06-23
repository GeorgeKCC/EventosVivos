
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Transversal.Cache.CacheVersion;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Transversal.Cache
{
    public static class CacheServices
    {
        public static IServiceCollection CacheRegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICacheVersionService, CacheVersionService>();

            var redisConnection = configuration.GetConnectionString("RedisConnection")
                ?? throw new InvalidOperationException("Redis connection string not found in configuration.");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(redisConnection, true);

                config.AbortOnConnectFail = false;
                config.ConnectRetry = 3;
                config.ReconnectRetryPolicy = new ExponentialRetry(5000);
                config.KeepAlive = 180;
                config.SyncTimeout = 5000;

                var multiplexer = ConnectionMultiplexer.Connect(config);

                var logger = sp.GetService<ILogger<ConnectionMultiplexer>>();
                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger.LogInformation("Redis connection established to: {Endpoints}",
                        string.Join(", ", multiplexer.GetEndPoints()));
                }

                return multiplexer;
            });

            services.AddSingleton<IDistributedCache>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return new RedisCache(new RedisCacheOptions
                {
                    ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer),
                    InstanceName = "app:"
                });
            });

            services.AddSingleton<IFusionCacheBackplane>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                var logger = sp.GetService<ILogger<RedisBackplane>>();

                return new RedisBackplane(
                    Microsoft.Extensions.Options.Options.Create(new RedisBackplaneOptions
                    {
                        ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer)
                    }),
                    logger
                );
            });

            services.AddMemoryCache();

            services.AddFusionCache()
                    .WithDefaultEntryOptions(new FusionCacheEntryOptions
                    {
                        Duration = TimeSpan.FromMinutes(2),

                        IsFailSafeEnabled = true,
                        FailSafeMaxDuration = TimeSpan.FromMinutes(30),

                        EagerRefreshThreshold = 0.8f,

                        LockTimeout = TimeSpan.FromSeconds(2),

                        FactorySoftTimeout = TimeSpan.FromMilliseconds(500),
                        FactoryHardTimeout = TimeSpan.FromSeconds(2)
                    })

                    .WithSerializer(new FusionCacheSystemTextJsonSerializer())

                    .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())

                    .WithBackplane(sp => sp.GetRequiredService<IFusionCacheBackplane>());

            return services;
        }
    }
}
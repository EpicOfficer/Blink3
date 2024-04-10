using Blink3.Core.Caching.Memory;
using Blink3.Core.Caching.Redis;
using Blink3.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.Core.Caching.Extensions;

/// <summary>
///     Contains extension methods for configuring caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds caching to the specified <see cref="IServiceCollection" /> based on the provided configuration.
    ///     If a Redis connection string is provided in the <paramref name="config" /> parameter,
    ///     Redis caching service will be added to the service collection along with <see cref="ICachingService" /> interface.
    ///     Otherwise, memory caching service will be added.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the caching services to.</param>
    /// <param name="config">The <see cref="BlinkConfiguration" /> object that contains the Redis connection string.</param>
    public static void AddCaching(this IServiceCollection services, BlinkConfiguration config)
    {
        if (!string.IsNullOrWhiteSpace(config.Redis?.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options => { options.Configuration = config.Redis.ConnectionString; });
            services.AddSingleton<ICachingService, RedisCachingService>();
            return;
        }

        services.AddMemoryCache();
        services.AddSingleton<ICachingService, MemoryCachingService>();
    }
}
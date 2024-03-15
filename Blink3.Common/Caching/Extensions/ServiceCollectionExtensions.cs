using Blink3.Common.Caching;
using Blink3.Common.Caching.Memory;
using Blink3.Common.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.Common.Caching.Extensions;

/// <summary>
/// Contains extension methods for configuring caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds caching support to the <see cref="IServiceCollection"/>.
    /// Will use Redis if configured, falling back to in-memory caching.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add caching to.</param>
    /// <param name="config">The configuration, optionally containing the Redis connection string.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration config)
    {
        string? redisConnectionString = config.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
            services.AddTransient<ICachingService, RedisCachingService>();

            return services;
        }
        
        services.AddMemoryCache();
        services.AddTransient<ICachingService, MemoryCachingService>();

        return services;
    }
}
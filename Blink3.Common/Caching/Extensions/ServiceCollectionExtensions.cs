using Blink3.Common.Caching;
using Blink3.Common.Caching.Memory;
using Blink3.Common.Caching.Redis;
using Blink3.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.Common.Caching.Extensions;

/// <summary>
/// Contains extension methods for configuring caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds caching to the specified <see cref="IServiceCollection"/> based on the provided configuration.
    /// If a Redis connection string is provided in the <paramref name="config"/> parameter,
    /// Redis caching service will be added to the service collection along with <see cref="ICachingService"/> interface.
    /// Otherwise, memory caching service will be added.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the caching services to.</param>
    /// <param name="config">The <see cref="BlinkConfiguration"/> object that contains the Redis connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/> with the caching services added.</returns>
    public static IServiceCollection AddCaching(this IServiceCollection services, BlinkConfiguration config)
    {
        if (!string.IsNullOrWhiteSpace(config.Redis?.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.Redis.ConnectionString;
            });
            services.AddSingleton<ICachingService, RedisCachingService>();

            return services;
        }
        
        services.AddMemoryCache();
        services.AddSingleton<ICachingService, MemoryCachingService>();

        return services;
    }
}
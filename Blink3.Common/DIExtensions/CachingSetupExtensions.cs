using Blink3.Common.Caching;
using Blink3.Common.Caching.Memory;
using Blink3.Common.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.Common.DIExtensions;

public static class CachingSetupExtensions
{
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
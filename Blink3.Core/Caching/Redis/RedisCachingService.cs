using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Blink3.Core.Caching.Redis;

/// <inheritdoc />
public class RedisCachingService(IDistributedCache cache) : ICachingService
{
    public async Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1)
        };

        string jsonData = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, jsonData, options).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        string? jsonData = await cache.GetStringAsync(key).ConfigureAwait(false);

        return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key).ConfigureAwait(false);
    }
}
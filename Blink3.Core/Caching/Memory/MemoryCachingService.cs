using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Blink3.Core.Caching.Memory;

/// <inheritdoc />
public class MemoryCachingService(IMemoryCache cache) : ICachingService
{
    public async Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null)
    {
        MemoryCacheEntryOptions cacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1)
        };

        cache.Set(key, value, cacheEntryOptions);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (cache.TryGetValue(key, out T? value)) return await Task.FromResult(value).ConfigureAwait(false);

        return default;
    }

    public async Task RemoveAsync(string key)
    {
        cache.Remove(key);
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
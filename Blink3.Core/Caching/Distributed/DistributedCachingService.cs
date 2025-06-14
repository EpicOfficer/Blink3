using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Blink3.Core.Caching.Distributed;

/// <inheritdoc />
public class DistributedCachingService(IDistributedCache cache) : ICachingService
{
    public async Task SetAsync(string key, object? value, TimeSpan? absoluteExpireTime = null,
        CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1)
        };

        string jsonData = JsonConvert.SerializeObject(value);

        await cache.SetStringAsync(key, jsonData, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? jsonData = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);

        return jsonData is null ? default : JsonConvert.DeserializeObject<T>(jsonData);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getter, TimeSpan? absoluteExpireTime = null,
        CancellationToken cancellationToken = default)
    {
        // Try to get the value from the cache
        T? result = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);

        // If not found, call getter and save the result in cache
        if (result is not null) return result;

        result = await getter();
        await SetAsync(key, result, absoluteExpireTime, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }
}
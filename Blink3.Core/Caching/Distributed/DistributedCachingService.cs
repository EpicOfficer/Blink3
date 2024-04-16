using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Blink3.Core.Caching.Distributed;

/// <inheritdoc />
public class DistributedCachingService(IDistributedCache cache) : ICachingService
{
    public async Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null,
        CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1)
        };

        string jsonData = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, jsonData, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? jsonData = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);

        return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }
}
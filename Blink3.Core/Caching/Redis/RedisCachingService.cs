using System.Text.Json;
using Blink3.Core.Helpers;
using Microsoft.Extensions.Caching.Distributed;

namespace Blink3.Core.Caching.Redis;

/// <inheritdoc />
public class RedisCachingService : ICachingService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedisCachingService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new ImageSharpColorConverter());
    }
    
    public async Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null, CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1)
        };
        
        string jsonData = JsonSerializer.Serialize(value, _jsonOptions);

        await _cache.SetStringAsync(key, jsonData, options, token: cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? jsonData = await _cache.GetStringAsync(key, token: cancellationToken).ConfigureAwait(false);

        return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData, _jsonOptions);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }
}
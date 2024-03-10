namespace Blink3.Common.Caching;

public interface ICachingService
{
    Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}
namespace Blink3.Common.Caching;

/// <summary>
/// Represents a caching service that provides methods to interact with a cache.
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Asynchronously sets a value in the cache with the specified key and optional expiration time.
    /// </summary>
    /// <param name="key">The cache key for the value.</param>
    /// <param name="value">The value to be cached.</param>
    /// <param name="absoluteExpireTime">Optional. The absolute expiration time for the cached value. If not specified, the default expiration time is 1 hour.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAsync(string key, object value, TimeSpan? absoluteExpireTime = null);

    /// <summary>
    /// Retrieves the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>
    /// The value associated with the specified key, or default(T) if the key is not found.
    /// </returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Removes the cached item with the specified key.
    /// </summary>
    /// <param name="key">The key of the item to remove from the cache.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string key);
}
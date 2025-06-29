namespace Blink3.Core.Caching;

/// <summary>
///     Represents a caching service that provides methods to interact with a cache.
/// </summary>
public interface ICachingService
{
    /// <summary>
    ///     Asynchronously sets a value in the cache with the specified key and optional expiration time.
    /// </summary>
    /// <param name="key">The cache key for the value.</param>
    /// <param name="value">The value to be cached.</param>
    /// <param name="absoluteExpireTime">
    ///     Optional. The absolute expiration time for the cached value. If not specified, the
    ///     default expiration time is 1 hour.
    /// </param>
    /// <param name="cancellationToken">Optional. The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAsync(string key, object? value, TimeSpan? absoluteExpireTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The value associated with the specified key, or default(T) if the key is not found.
    /// </returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the value associated with the specified key, or adds it to the cache if not found.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="getter">The function to call to obtain the value if not found in the cache.</param>
    /// <param name="absoluteExpireTime">
    ///     Optional. The absolute expiration time for the cached value. If not specified, the
    ///     default expiration time is 1 hour.
    /// </param>
    /// <param name="cancellationToken">Optional. The cancellation token to cancel the operation.</param>
    /// <returns>The value associated with the specified key, or the newly added value if not found in the cache.</returns>
    public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getter, TimeSpan? absoluteExpireTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the cached item with the specified key.
    /// </summary>
    /// <param name="key">The key of the item to remove from the cache.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
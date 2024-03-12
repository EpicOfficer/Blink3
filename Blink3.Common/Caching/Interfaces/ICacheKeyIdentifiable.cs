namespace Blink3.Common.Caching.Interfaces;

/// <summary>
/// Represents an object that can be used as a cache key identifier.
/// </summary>
public interface ICacheKeyIdentifiable
{
    /// <summary>
    /// Returns the cache key for the entity.
    /// </summary>
    /// <returns>The cache key for the entity.</returns>
    string GetCacheKey();
}
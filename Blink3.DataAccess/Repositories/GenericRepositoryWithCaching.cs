using Blink3.Core.Caching;
using Blink3.Core.Caching.Interfaces;

namespace Blink3.DataAccess.Repositories;

/// <summary>
///     The GenericRepositoryWithCaching class provides a generic repository implementation with caching capability.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class GenericRepositoryWithCaching<T>(BlinkDbContext dbContext, ICachingService cache)
    : GenericRepository<T>(dbContext) where T : class, ICacheKeyIdentifiable, new()
{
    /// <summary>
    ///     Generates a cache key for the specified entity.
    /// </summary>
    /// <param name="keyValues">The key values used to uniquely identify the entity.</param>
    /// <returns>The cache key as a string.</returns>
    private static string GetCacheKey(object[] keyValues)
    {
        if (keyValues.Length == 0) throw new ArgumentException("No valid key provided", nameof(keyValues));

        string key = string.Join(":", keyValues.Select(k => k.ToString()));
        return $"{nameof(T)}:{key}";
    }

    /// <summary>
    ///     Gets the cache key for the specified entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The cache key for the entity.</returns>
    private static string GetCacheKeyFromEntity(T entity)
    {
        return $"{nameof(T)}:{entity.GetCacheKey()}";
    }

    /// <summary>
    ///     Sets the specified entity in the cache.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to be set in the cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetEntityInCache(T entity, CancellationToken cancellationToken = default)
    {
        string cacheKey = GetCacheKeyFromEntity(entity);
        await cache.SetAsync(cacheKey, entity, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Removes the specified entity from the cache.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to remove from the cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RemoveEntityFromCache(T entity, CancellationToken cancellationToken = default)
    {
        string cacheKey = GetCacheKeyFromEntity(entity);
        await cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        string cacheKey = GetCacheKey(keyValues);

        T? entity = await cache.GetAsync<T>(cacheKey).ConfigureAwait(false);
        if (entity != null) return entity;

        entity = await base.GetByIdAsync(keyValues).ConfigureAwait(false);

        // Update cache
        if (entity != null) await SetEntityInCache(entity).ConfigureAwait(false);

        return entity;
    }

    public override async Task<T> GetOrCreateByIdAsync(params object[] keyValues)
    {
        string cacheKey = GetCacheKey(keyValues);
        
        T? entity = await cache.GetAsync<T>(cacheKey).ConfigureAwait(false);
        if (entity is not null) return entity;

        entity = await base.GetOrCreateByIdAsync(keyValues).ConfigureAwait(false);
        
        await SetEntityInCache(entity);
        
        return entity;
    }

    public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        T updatedEntity = await base.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await SetEntityInCache(updatedEntity, cancellationToken).ConfigureAwait(false);
        return updatedEntity;
    }

    public override async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        T updatedEntity = await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await SetEntityInCache(updatedEntity, cancellationToken).ConfigureAwait(false);
        return updatedEntity;
    }

    public override async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await RemoveEntityFromCache(entity, cancellationToken).ConfigureAwait(false);
    }

    public override async Task DeleteByIdAsync(params object[] keyValues)
    {
        string cacheKey = GetCacheKey(keyValues);
        await base.DeleteByIdAsync(keyValues).ConfigureAwait(false);
        await cache.RemoveAsync(cacheKey).ConfigureAwait(false);
    }
}
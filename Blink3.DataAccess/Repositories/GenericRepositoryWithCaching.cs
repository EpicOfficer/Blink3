using Blink3.Common.Caching;
using Blink3.Common.Caching.Interfaces;

namespace Blink3.DataAccess.Repositories;

/// <summary>
/// The GenericRepositoryWithCaching class provides a generic repository implementation with caching capability.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class GenericRepositoryWithCaching<T>(BlinkDbContext dbContext, ICachingService cache) : GenericRepository<T>(dbContext) where T : class, ICacheKeyIdentifiable
{
    private static string GetCacheKey(object[] keyValues)
    {
        if (keyValues.Length == 0)
        {
            throw new ArgumentException("No valid key provided", nameof(keyValues));
        }

        string key = string.Join(":", keyValues.Select(k => k.ToString()));
        return $"{nameof(T)}:{key}";
    }

    private static string GetCacheKeyFromEntity(T entity) => $"{nameof(T)}:{entity.GetCacheKey()}";
    
    private async Task SetEntityInCache(T entity)
    {
        string cacheKey = GetCacheKeyFromEntity(entity);
        await cache.SetAsync(cacheKey, entity);
    }

    private async Task RemoveEntityFromCache(T entity)
    {
        string cacheKey = GetCacheKeyFromEntity(entity);
        await cache.RemoveAsync(cacheKey);
    }

    public override async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        string cacheKey = GetCacheKey(keyValues);
        
        T? entity = await cache.GetAsync<T>(cacheKey);
        if (entity != null) return entity;
        
        entity = await base.GetByIdAsync(keyValues);
        
        // Update cache
        if (entity != null)
        {
            await SetEntityInCache(entity);
        }
        
        return entity;
    }

    public override async Task<T> AddAsync(T entity)
    {
        T updatedEntity = await base.AddAsync(entity);
        await SetEntityInCache(updatedEntity);
        return updatedEntity;
    }

    public override async Task<T> UpdateAsync(T entity)
    {
        T updatedEntity = await base.UpdateAsync(entity);
        await SetEntityInCache(updatedEntity);
        return updatedEntity;
    }

    public override async Task DeleteAsync(T entity)
    {
        await base.DeleteAsync(entity);
        await RemoveEntityFromCache(entity);
    }
}
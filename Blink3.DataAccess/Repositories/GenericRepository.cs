using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IGenericRepository{T}" />
public class GenericRepository<T>(BlinkDbContext dbContext)
    : IGenericRepository<T> where T : class, new()
{
    public virtual async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        return await dbContext.Set<T>().FindAsync(keyValues).ConfigureAwait(false);
    }

    public virtual async Task<T> GetOrCreateByIdAsync(params object[] keyValues)
    {
        T? entity = await GetByIdAsync(keyValues).ConfigureAwait(false);
        if (entity is not null) return entity;

        (IEntityType? entityType, IKey? key) = GetEntityTypeAndKey();
        entity = CreateEntityWithKeys(key, keyValues);

        dbContext.Entry(entity).State = EntityState.Detached;

        await AddAsync(entity).ConfigureAwait(false);

        return entity;
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Entry(entity).State = EntityState.Modified;

        return entity;
    }

    public virtual Task<T> UpdatePropertiesAsync(T entity, params Action<T>[] updatedProperties)
    {
        dbContext.Set<T>().Attach(entity);
        foreach (Action<T> property in updatedProperties) property(entity);
        
        return Task.FromResult(entity);
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Attach(entity);
        dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteByIdAsync(params object[] keyValues)
    {
        (_, IKey? key) = GetEntityTypeAndKey();
        T entity = CreateEntityWithKeys(key, keyValues);
        dbContext.Entry(entity).State = EntityState.Detached;
        await DeleteAsync(entity).ConfigureAwait(false);
    }

    private (IEntityType? entityType, IKey? key) GetEntityTypeAndKey()
    {
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(T));
        IKey? key = entityType?.FindPrimaryKey();

        return (entityType, key);
    }

    private T CreateEntityWithKeys(IKey? key, params object[] keyValues)
    {
        if (key?.Properties.Count != keyValues.Length)
            throw new ArgumentException("Number of key values do not match number of key properties");

        T entity = new();
        EntityEntry<T> entry = dbContext.Entry(entity);

        if (key.Properties.Count != keyValues.Length)
            throw new ArgumentException(
                $"Invalid number of key values. Expected {key.Properties.Count} but got {keyValues.Length}.");

        for (int i = 0; i < key.Properties.Count; i++)
        {
            if (key.Properties[i].ClrType != keyValues[i].GetType())
                throw new ArgumentException(
                    $"Mismatched key type at position {i}. Expected {key.Properties[i].GetType()} but got {keyValues[i].GetType()}.");

            entry.Property(key.Properties[i].Name).CurrentValue = keyValues[i];
        }

        return entity;
    }
}
using Blink3.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IGenericRepository{T}"/>
public class GenericRepository<T>(BlinkDbContext dbContext) : IGenericRepository<T> where T : class
{
    public virtual async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        if (keyValues.Length == 0)
            return default;

        return await dbContext.Set<T>().FindAsync(keyValues);
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await dbContext.Set<T>().ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangesAsync();

        return entity;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        dbContext.Entry(entity).State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task DeleteByIdAsync(params object[] keyValues)
    {
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(T));
        IKey? key = entityType?.FindPrimaryKey();

        if (key?.Properties.Count != keyValues.Length)
        {
            throw new Exception("Number of key values do not match number of key properties");
        }

        EntityEntry<T> entry = dbContext.Entry(Activator.CreateInstance<T>());
        for (int i = 0; i < key.Properties.Count; i++)
        {
            entry.Property(key.Properties[i].Name).CurrentValue = keyValues[i];
        }

        dbContext.Set<T>().Attach(entry.Entity);
        dbContext.Set<T>().Remove(entry.Entity);

        await dbContext.SaveChangesAsync();
    }
}
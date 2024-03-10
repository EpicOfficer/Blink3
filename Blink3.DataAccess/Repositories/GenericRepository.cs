using Blink3.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

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
}
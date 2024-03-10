using Blink3.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class GenericRepository<T>(BlinkDbContext dbContext) : IGenericRepository<T> where T : class
{
    public async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        if (keyValues.Length == 0)
            return default;

        return await dbContext.Set<T>().FindAsync(keyValues).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await dbContext.Set<T>().ToListAsync().ConfigureAwait(false);
    }

    public async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        dbContext.Entry(entity).State = EntityState.Deleted;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
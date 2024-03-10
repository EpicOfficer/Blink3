namespace Blink3.DataAccess.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
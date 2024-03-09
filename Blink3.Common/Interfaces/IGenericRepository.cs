namespace Blink3.Common.Interfaces;

public interface IGenericRepository<T> : IReadOnlyRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
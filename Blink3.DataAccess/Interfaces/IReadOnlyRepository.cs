namespace Blink3.DataAccess.Interfaces;

public interface IReadOnlyRepository<T> where T : class
{
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task<IReadOnlyCollection<T>> GetAllAsync();
}